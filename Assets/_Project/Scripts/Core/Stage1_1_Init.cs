using UnityEngine;
using ProjectKai.Data;
using ProjectKai.UI;

namespace ProjectKai.Core
{
    /// <summary>
    /// 1-1 슬럼 외곽 스테이지 초기화.
    /// 도입 대사 + 클리어 대사 설정.
    /// </summary>
    public class Stage1_1_Init : MonoBehaviour
    {
        private void Start()
        {
            // 도입 대사
            var introLines = new DialogueLine[]
            {
                new DialogueLine { speakerName = "리나", text = "카이, 간단한 일이야. 폐허 던전에 고블린이 늘었대. 청소 의뢰." },
                new DialogueLine { speakerName = "카이", text = "보수는?" },
                new DialogueLine { speakerName = "리나", text = "2만 골드. 쉬운 돈이잖아." },
                new DialogueLine { speakerName = "카이", text = "슬럼 아이들이 위험하다고?" },
                new DialogueLine { speakerName = "리나", text = "...그건 의뢰에 없어." },
                new DialogueLine { speakerName = "카이", text = "됐어. 가자." }
            };

            // 약간 대기 후 대화 시작 (시스템 초기화 대기)
            StartCoroutine(ShowIntroDelayed(introLines));
        }

        private System.Collections.IEnumerator ShowIntroDelayed(DialogueLine[] lines)
        {
            yield return new WaitForSeconds(0.5f);
            DialogueSystem.Instance?.StartDialogue(lines);
        }
    }
}
