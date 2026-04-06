using UnityEngine;
using ProjectKai.Data;
using ProjectKai.UI;
using ProjectKai.Combat;

namespace ProjectKai.Core
{
    public class Stage1_3_Init : MonoBehaviour
    {
        private bool _bossDefeated;

        private void Start()
        {
            // 보스 사망 이벤트 구독
            var boss = GameObject.FindWithTag("Enemy");
            if (boss != null)
            {
                var dr = boss.GetComponent<DamageReceiver>();
                if (dr != null)
                    dr.OnDeath += OnBossDefeated;
            }
        }

        private void OnBossDefeated()
        {
            if (_bossDefeated) return;
            _bossDefeated = true;

            // 2장 해금
            GameState.Instance?.UnlockChapter(2);

            StartCoroutine(BossDefeatSequence());
        }

        private System.Collections.IEnumerator BossDefeatSequence()
        {
            yield return new WaitForSeconds(2f);

            var clearLines = new DialogueLine[]
            {
                new DialogueLine { speakerName = "카이", text = "이 문양... 그림자 기사단?" },
                new DialogueLine { speakerName = "리나", text = "카이, 이거 단순한 의뢰가 아니었어. 이 던전, 그림자 기사단의 실험장이야." },
                new DialogueLine { speakerName = "카이", text = "...보수 올려." },
                new DialogueLine { speakerName = "리나", text = "이건 용병이 건드릴 수준이 아니야. 빠지자." },
                new DialogueLine { speakerName = "카이", text = "못 빠져. 이 아이들 누가 지켜?" },
                new DialogueLine { speakerName = "리나", text = "...또 시작이야, 돈키호테 선생님." },
                new DialogueLine { speakerName = "카이", text = "풍차가 아니면 좋겠지만." }
            };

            DialogueSystem.Instance?.StartDialogue(clearLines);
        }
    }
}
