using UnityEngine;
using System.IO;
using System.Collections;

namespace ProjectKai.Core
{
    /// <summary>
    /// QA 테스트 녹화: Play 모드 중 0.2초 간격으로 스크린샷을 저장.
    /// 테스트 종료 시 캡처 폴더 경로를 출력.
    /// </summary>
    public class QARecorder : MonoBehaviour
    {
        public static QARecorder Instance { get; private set; }

        [SerializeField] private float _captureInterval = 0.2f;
        [SerializeField] private bool _autoStartOnPlay = true;

        private string _outputFolder;
        private int _frameCount;
        private bool _isRecording;
        private float _timer;
        private float _startTime;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (_autoStartOnPlay)
                StartRecording();
        }

        /// <summary>
        /// 에디터 스크립트에서 Play 전에 폴더를 미리 설정
        /// </summary>
        public void SetOutputFolder(string folder)
        {
            _outputFolder = folder;
        }

        public void StartRecording()
        {
            if (string.IsNullOrEmpty(_outputFolder))
            {
                _outputFolder = Path.Combine(Application.dataPath, "..", "QA_Recordings",
                    System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            }
            Directory.CreateDirectory(_outputFolder);

            _frameCount = 0;
            _isRecording = true;
            _startTime = Time.time;

            Debug.Log($"[QARecorder] 녹화 시작: {_outputFolder}");
        }

        public void StopRecording()
        {
            _isRecording = false;
            float duration = Time.time - _startTime;
            Debug.Log($"[QARecorder] 녹화 종료! {_frameCount}장, {duration:F1}초, 폴더: {_outputFolder}");
        }

        private void LateUpdate()
        {
            if (!_isRecording) return;

            _timer += Time.unscaledDeltaTime;
            if (_timer >= _captureInterval)
            {
                _timer -= _captureInterval;
                StartCoroutine(CaptureFrame());
            }
        }

        private IEnumerator CaptureFrame()
        {
            yield return new WaitForEndOfFrame();

            if (string.IsNullOrEmpty(_outputFolder)) yield break;

            string filename = Path.Combine(_outputFolder, $"frame_{_frameCount:D4}.png");
            ScreenCapture.CaptureScreenshot(filename);
            _frameCount++;
        }

        private void OnDestroy()
        {
            if (_isRecording)
                StopRecording();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (Instance == null)
            {
                var obj = new GameObject("QARecorder");
                obj.AddComponent<QARecorder>();
            }
        }
    }
}
