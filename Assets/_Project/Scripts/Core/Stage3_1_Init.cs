using UnityEngine;
using ProjectKai.Data;
using ProjectKai.UI;

namespace ProjectKai.Core
{
    public class Stage3_1_Init : MonoBehaviour
    {
        private void Start()
        {
            GameState.Instance.CurrentChapter = 3;

            var lines = new DialogueLine[]
            {
                new DialogueLine { speakerName = "리나", text = "기사단 본거지... 정말 가는 거야?" },
                new DialogueLine { speakerName = "카이", text = "가야지. 여기서 멈추면 슬럼 아이들은?" },
                new DialogueLine { speakerName = "리나", text = "슬럼 사람들이 문을 열어줬어. 네가 바꾼 거야." },
                new DialogueLine { speakerName = "카이", text = "아직 안 바꿨어. 이제부터 바꾸는 거지." },
                new DialogueLine { speakerName = "리나", text = "...돈키호테 선생님, 힘내." }
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
