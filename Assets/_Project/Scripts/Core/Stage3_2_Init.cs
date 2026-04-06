using UnityEngine;
using ProjectKai.Data;
using ProjectKai.UI;

namespace ProjectKai.Core
{
    public class Stage3_2_Init : MonoBehaviour
    {
        private void Start()
        {
            var lines = new DialogueLine[]
            {
                new DialogueLine { speakerName = "카이", text = "여기는... 호화롭군. 에테르로 만든 세상이 이런 건가." },
                new DialogueLine { speakerName = "리나", text = "슬럼과는 완전 다른 세상이네." },
                new DialogueLine { speakerName = "카이", text = "이게 기사단이 원하는 세상이야. 자기들만의." },
                new DialogueLine { speakerName = "카이", text = "바꿔야 해." }
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
