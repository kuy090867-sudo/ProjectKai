using UnityEngine;
using ProjectKai.Data;
using ProjectKai.UI;

namespace ProjectKai.Core
{
    public class Stage2_2_Init : MonoBehaviour
    {
        private void Start()
        {
            var lines = new DialogueLine[]
            {
                new DialogueLine { speakerName = "카이", text = "연구소... 여기서 에테르 실험을 한 거야?" },
                new DialogueLine { speakerName = "리나", text = "이 장비들... 우리 부모님이 쓰던 것과 같아." },
                new DialogueLine { speakerName = "카이", text = "리나..." },
                new DialogueLine { speakerName = "리나", text = "괜찮아. 진실을 알아야 하니까. 계속 가자." }
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
