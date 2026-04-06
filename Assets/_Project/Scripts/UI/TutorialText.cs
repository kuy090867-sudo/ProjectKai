using UnityEngine;
using TMPro;

namespace ProjectKai.UI
{
    /// <summary>
    /// 튜토리얼 텍스트. 플레이어가 가까이 오면 표시, 멀어지면 사라짐.
    /// 1-1 시작 구간에 배치하여 조작법 안내.
    /// </summary>
    public class TutorialText : MonoBehaviour
    {
        [SerializeField] private string _text = "A/D: 이동";
        [SerializeField] private float _showRange = 3f;

        private TextMeshPro _tmp;
        private Transform _player;
        private bool _shown;

        private void Start()
        {
            _tmp = gameObject.AddComponent<TextMeshPro>();
            _tmp.text = _text;
            _tmp.fontSize = 3f;
            _tmp.color = new Color(1f, 1f, 1f, 0f);
            _tmp.alignment = TextAlignmentOptions.Center;
            _tmp.sortingOrder = 5;
        }

        private void Update()
        {
            if (_player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) _player = p.transform;
                return;
            }

            float dist = Vector2.Distance(transform.position, _player.position);
            float targetAlpha = dist < _showRange ? 0.8f : 0f;
            var c = _tmp.color;
            c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * 5f);
            _tmp.color = c;
        }
    }
}
