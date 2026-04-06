using UnityEngine;
using ProjectKai.Data;
using ProjectKai.UI;

namespace ProjectKai.Core
{
    public class Stage1_2_Init : MonoBehaviour
    {
        private void Start()
        {
            var introLines = new DialogueLine[]
            {
                new DialogueLine { speakerName = "카이", text = "여기서부터 분위기가 다르군. 에테르 냄새가 난다." },
                new DialogueLine { speakerName = "리나", text = "조심해. 이 아래쪽은 지도에도 없는 구역이야." }
            };

            StartCoroutine(ShowDelayed(introLines));
        }

        private System.Collections.IEnumerator ShowDelayed(DialogueLine[] lines)
        {
            yield return new WaitForSeconds(0.5f);
            DialogueSystem.Instance?.StartDialogue(lines);
        }
    }
}
