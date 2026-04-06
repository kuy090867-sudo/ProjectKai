#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace ProjectKai.Editor
{
    /// <summary>
    /// 에디터에서 Play 누르기 직전 상태를 캡처.
    /// Play 진입 시 자동으로 Game View 스크린샷 시작.
    /// </summary>
    [InitializeOnLoad]
    public static class QARecorderEditor
    {
        private static string _outputFolder;

        static QARecorderEditor()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    // Play 누르기 직전 — 에디터 상태 캡처
                    _outputFolder = Path.Combine(
                        Application.dataPath, "..", "QA_Recordings",
                        System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    Directory.CreateDirectory(_outputFolder);

                    // Scene View 스크린샷
                    string beforePath = Path.Combine(_outputFolder, "BEFORE_PLAY.png");
                    ScreenCapture.CaptureScreenshot(beforePath);
                    Debug.Log($"[QA] Play 전 상태 캡처: {beforePath}");
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    // Play 진입 — QARecorder에 폴더 경로 전달
                    var recorder = Object.FindFirstObjectByType<Core.QARecorder>();
                    if (recorder != null)
                    {
                        recorder.SetOutputFolder(_outputFolder);
                        Debug.Log($"[QA] 녹화 폴더 연동: {_outputFolder}");
                    }
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    Debug.Log($"[QA] Play 종료. 녹화 폴더: {_outputFolder}");
                    break;
            }
        }
    }
}
#endif
