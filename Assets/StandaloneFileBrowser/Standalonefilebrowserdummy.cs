#if ENABLE_IL2CPP && !UNITY_STANDALONE_WIN && !UNITY_EDITOR

using System;
using UnityEngine;

namespace SFB {
    // Заглушка для IL2CPP-билдов — нативные диалоги не поддерживаются.
    // Все методы возвращают пустые результаты.
    public class StandaloneFileBrowserDummy : IStandaloneFileBrowser {
        public string[] OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect) {
            Debug.LogWarning("StandaloneFileBrowser: file dialogs are not supported in IL2CPP builds.");
            return new string[0];
        }

        public string[] OpenFolderPanel(string title, string directory, bool multiselect) {
            Debug.LogWarning("StandaloneFileBrowser: file dialogs are not supported in IL2CPP builds.");
            return new string[0];
        }

        public string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions) {
            Debug.LogWarning("StandaloneFileBrowser: file dialogs are not supported in IL2CPP builds.");
            return "";
        }

        public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<string[]> cb) {
            Debug.LogWarning("StandaloneFileBrowser: file dialogs are not supported in IL2CPP builds.");
            cb.Invoke(new string[0]);
        }

        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<string[]> cb) {
            Debug.LogWarning("StandaloneFileBrowser: file dialogs are not supported in IL2CPP builds.");
            cb.Invoke(new string[0]);
        }

        public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb) {
            Debug.LogWarning("StandaloneFileBrowser: file dialogs are not supported in IL2CPP builds.");
            cb.Invoke("");
        }
    }
}

#endif