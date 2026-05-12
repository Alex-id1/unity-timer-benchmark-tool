#if UNITY_STANDALONE_WIN && ENABLE_IL2CPP

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SFB {
    /// <summary>
    /// Native Windows file dialogs for IL2CPP builds via comdlg32.dll P/Invoke.
    /// Replaces WinForms-based StandaloneFileBrowserWindows which is unavailable in IL2CPP.
    /// </summary>
    public class StandaloneFileBrowserWindowsIL2CPP : IStandaloneFileBrowser {

        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool GetSaveFileNameW(ref OPENFILENAME ofn);

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct OPENFILENAME {
            public int    lStructSize;
            public IntPtr hwndOwner;
            public IntPtr hInstance;
            public string lpstrFilter;
            public string lpstrCustomFilter;
            public int    nMaxCustFilter;
            public int    nFilterIndex;
            public IntPtr lpstrFile;
            public int    nMaxFile;
            public IntPtr lpstrFileTitle;
            public int    nMaxFileTitle;
            public string lpstrInitialDir;
            public string lpstrTitle;
            public int    Flags;
            public short  nFileOffset;
            public short  nFileExtension;
            public string lpstrDefExt;
            public IntPtr lCustData;
            public IntPtr lpfnHook;
            public string lpTemplateName;
            public IntPtr pvReserved;
            public int    dwReserved;
            public int    FlagsEx;
        }

        private const int OFN_OVERWRITEPROMPT = 0x00000002;
        private const int OFN_PATHMUSTEXIST   = 0x00000800;
        private const int MAX_PATH            = 260;

        // -----------------------------------------
        //  Save
        // -----------------------------------------

        public string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions) {
            IntPtr fileBuffer = Marshal.AllocHGlobal(MAX_PATH * 2);
            try {
                // Zero-initialize buffer
                for (int i = 0; i < MAX_PATH * 2; i++)
                    Marshal.WriteByte(fileBuffer, i, 0);

                // Write default filename into buffer
                if (!string.IsNullOrEmpty(defaultName)) {
                    byte[] nameBytes = Encoding.Unicode.GetBytes(defaultName);
                    Marshal.Copy(nameBytes, 0, fileBuffer, Math.Min(nameBytes.Length, (MAX_PATH - 1) * 2));
                }

                var ofn = new OPENFILENAME();
                ofn.lStructSize     = Marshal.SizeOf(typeof(OPENFILENAME));
                ofn.hwndOwner       = GetActiveWindow();
                ofn.lpstrFile       = fileBuffer;
                ofn.nMaxFile        = MAX_PATH;
                ofn.lpstrTitle      = title;
                ofn.lpstrInitialDir = directory;
                ofn.Flags           = OFN_OVERWRITEPROMPT | OFN_PATHMUSTEXIST;

                if (extensions != null && extensions.Length > 0) {
                    ofn.lpstrFilter  = BuildFilter(extensions);
                    ofn.lpstrDefExt  = extensions[0].Extensions[0];
                    ofn.nFilterIndex = 1;
                }

                bool ok = GetSaveFileNameW(ref ofn);
                return ok ? Marshal.PtrToStringUni(fileBuffer) : "";
            } finally {
                Marshal.FreeHGlobal(fileBuffer);
            }
        }

        public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb) {
            cb.Invoke(SaveFilePanel(title, directory, defaultName, extensions));
        }

        // -----------------------------------------
        //  Open (not used in this project)
        // -----------------------------------------

        public string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect) {
            return new string[0];
        }

        public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb) {
            cb.Invoke(new string[0]);
        }

        public string[] OpenFolderPanel(string title, string directory, bool multiselect) {
            return new string[0];
        }

        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb) {
            cb.Invoke(new string[0]);
        }

        // -----------------------------------------
        //  Helpers
        // -----------------------------------------

        // Builds filter string in Windows OPENFILENAME format: "Name\0*.ext\0\0"
        private static string BuildFilter(ExtensionFilter[] extensions) {
            var sb = new StringBuilder();
            foreach (var ext in extensions) {
                sb.Append(ext.Name.Length > 0 ? ext.Name : "Files");
                sb.Append('\0');
                sb.Append(string.Join(";", Array.ConvertAll(ext.Extensions, e => "*." + e)));
                sb.Append('\0');
            }
            sb.Append('\0');
            return sb.ToString();
        }
    }
}

#endif
