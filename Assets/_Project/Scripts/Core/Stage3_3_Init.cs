using UnityEngine;
using ProjectKai.Data;
using ProjectKai.UI;
using ProjectKai.Combat;

namespace ProjectKai.Core
{
    public class Stage3_3_Init : MonoBehaviour
    {
        private bool _bossDefeated;

        private void Start()
        {
            var boss = GameObject.FindWithTag("Enemy");
            if (boss != null)
            {
                var dr = boss.GetComponent<DamageReceiver>();
                if (dr != null)
                    dr.OnDeath += OnFinalBossDefeated;
            }
        }

        private void OnFinalBossDefeated()
        {
            if (_bossDefeated) return;
            _bossDefeated = true;
            StartCoroutine(EndingSequence());
        }

        private System.Collections.IEnumerator EndingSequence()
        {
            GameFeel.Instance?.KillSlowMotion(1f, 0.1f);
            GameState.Instance?.CompleteGame();
            SaveSystem.Save();
            yield return new WaitForSecondsRealtime(1.5f);

            var lines = new DialogueLine[]
            {
                new DialogueLine { speakerName = "기사단장", text = "...너는 바꿀 수 없어. 용병 하나가 뭘 할 수 있지?" },
                new DialogueLine { speakerName = "카이", text = "하나가 바꿀 수 없다는 거 알아. 그래도 시작은 하나부터잖아." },
                new DialogueLine { speakerName = "", text = "대균열 장치가 파괴되었다.\n하지만 세상이 크게 바뀌진 않았다." },
                new DialogueLine { speakerName = "", text = "에테르 기업은 여전히 존재하고,\n슬럼은 여전히 가난했다." },
                new DialogueLine { speakerName = "리나", text = "결국 뭐가 바뀐 거야?" },
                new DialogueLine { speakerName = "", text = "슬럼의 한 아이가 카이에게 손을 흔들었다." },
                new DialogueLine { speakerName = "카이", text = "저것만 바뀌었으면 됐어." },
                new DialogueLine { speakerName = "리나", text = "...바보." },
                new DialogueLine { speakerName = "카이", text = "그래, 라만차의 기사는 원래 바보야." },
                new DialogueLine { speakerName = "", text = "— END —\n\n그림자 기사단은... 시작에 불과했다." }
            };

            DialogueSystem.Instance?.StartDialogue(lines);

            while (DialogueSystem.Instance != null && DialogueSystem.Instance.IsActive)
                yield return null;

            yield return new WaitForSeconds(2f);
            UI.CreditsScreen.Show();
        }
    }
}
