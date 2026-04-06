using UnityEngine;
using ProjectKai.Data;
using ProjectKai.UI;

namespace ProjectKai.Core
{
    public class Stage2_1_Init : MonoBehaviour
    {
        private void Start()
        {
            GameState.Instance.CurrentChapter = 2;

            var lines = new DialogueLine[]
            {
                new DialogueLine { speakerName = "리나", text = "여기가 그림자 기사단의 외곽 기지야. 경비가 삼엄해." },
                new DialogueLine { speakerName = "카이", text = "스켈레톤... 마법으로 움직이는 사병이군." },
                new DialogueLine { speakerName = "리나", text = "오크 워리어도 보여. 조심해, 방패가 있어." },
                new DialogueLine { speakerName = "카이", text = "풍차치고는 좀 크다." }
            };
            StartCoroutine(Delayed(lines));
        }

        private System.Collections.IEnumerator Delayed(DialogueLine[] l)
        {
            yield return new WaitForSeconds(0.5f);
            DialogueSystem.Instance?.StartDialogue(l);
        }
    }
}
