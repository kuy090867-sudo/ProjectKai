using UnityEngine;
using ProjectKai.Data;
using ProjectKai.UI;

namespace ProjectKai.Core
{
    /// <summary>
    /// 환경 스토리텔링 — 던전 내 로어 노트.
    /// 접근 시 짧은 텍스트 표시 (Hollow Knight 스타일).
    /// </summary>
    public class LoreNote : MonoBehaviour
    {
        [SerializeField] private string _title = "낡은 메모";
        [TextArea(2, 5)]
        [SerializeField] private string _content = "...여기서 실험이 진행되었다. 에테르 수정의 힘은 상상 이상이다...";
        [SerializeField] private float _interactRange = 2f;

        private Transform _player;
        private bool _read;

        private void Update()
        {
            if (_read) return;

            if (_player == null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) _player = p.transform;
                return;
            }

            if (Vector2.Distance(transform.position, _player.position) < _interactRange)
            {
                _read = true;
                ShowNote();
            }
        }

        private void ShowNote()
        {
            var lines = new DialogueLine[]
            {
                new DialogueLine { speakerName = _title, text = _content }
            };
            DialogueSystem.Instance?.StartDialogue(lines);
        }
    }
}
