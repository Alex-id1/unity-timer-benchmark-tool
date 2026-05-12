using System;
using DG.Tweening;
using SFB;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the save-to-file UI panel: input fields for benchmark name and datetime toggle,
/// the save button that opens a native file dialog.
/// Invokes OnSaveBenchmarkBtnPressed with the chosen path for the presenter to handle
/// </summary>
public class ReporterView : MonoBehaviour {
    [Header("Save result")]
    [SerializeField] private RectTransform saveBenchmarkToFileGroupRT;
    [SerializeField] private InputField _testNameInputField;
    [SerializeField] private Toggle _dateTimeAllowedToggle;

    private Button _saveButton;
    private BenchmarkSavePathProvider _pathProvider;

    /// <summary>
    /// Invokes after the user picks a path in the save dialog. Argument - the chosen full file path (.csv)
    /// </summary>
    public static event Action<string> OnSaveBenchmarkBtnPressed;

    private bool FilePathDateTimeAllowed {
        get => PlayerPrefs.GetInt(PrefsKeyDateTime, 1) == 1;
        set => PlayerPrefs.SetInt(PrefsKeyDateTime, value ? 1 : 0);
    }
    private string TestName {
        get => PlayerPrefs.GetString(PrefsKeyTestName, InitialTestNameStr);
        set { if (!string.IsNullOrEmpty(value)) PlayerPrefs.SetString(PrefsKeyTestName, value); }
    }

    private const string PrefsKeyDateTime = "SaveFilePathDateTimeAllowed";
    private const string PrefsKeyTestName = "TestName";
    private const string InitialTestNameStr = "Timer Benchmark";
    private const string SaveDialogTitle = "Save benchmark";
    private const string CsvExtension = "csv";

    private const float SavePopupOpenY = -15f;
    private const float SavePopupClosedY = -180f;

    public void Init(BenchmarkSavePathProvider pathProvider) {
        _pathProvider = pathProvider;
    }

    private void Start() {
        saveBenchmarkToFileGroupRT.anchoredPosition = new Vector2(saveBenchmarkToFileGroupRT.anchoredPosition.x, SavePopupClosedY);

        _saveButton = saveBenchmarkToFileGroupRT.GetComponentInChildren<Button>();
        _saveButton.OnClickAsObservable().Subscribe(_ => ShowSaveDialog()).AddTo(this);

        _dateTimeAllowedToggle.isOn = FilePathDateTimeAllowed;
        _dateTimeAllowedToggle.OnValueChangedAsObservable().Subscribe(val => FilePathDateTimeAllowed = val).AddTo(this);

        _testNameInputField.text = TestName;
        _testNameInputField.OnValueChangedAsObservable().Subscribe(val => TestName = val).AddTo(this);
    }

    /// <summary>
    /// Opens a native save file dialog pre-filled with the default folder and file name.
    /// Remembers the chosen folder for the next session and invokes OnSaveBenchmarkBtnPressed
    /// </summary>
    private void ShowSaveDialog() {
        if (_pathProvider == null) {
            Debug.LogError("[ReporterView] BenchmarkSavePathProvider is not injected");
            return;
        }

        string folder = _pathProvider.DefaultFolder;
        string fileName = _pathProvider.BuildDefaultFileName(_testNameInputField.text, FilePathDateTimeAllowed);

        string path = StandaloneFileBrowser.SaveFilePanel(SaveDialogTitle, folder, fileName, CsvExtension);
        if (string.IsNullOrEmpty(path))
            return;

        _pathProvider.RememberFolder(path);
        OnSaveBenchmarkBtnPressed?.Invoke(path);
        CloseSaveUIGroup();
    }

    public void OpenSaveBenchmarkToFilePopup(float moveTime = 0.45f, float delay = 1f) {
        saveBenchmarkToFileGroupRT.DOAnchorPosY(SavePopupOpenY, moveTime).SetDelay(delay);
    }

    public void CloseSaveUIGroup(float moveTime = 0.45f, float delay = 0.5f) {
        saveBenchmarkToFileGroupRT.DOAnchorPosY(SavePopupClosedY, moveTime).SetDelay(delay);
    }
}