using UnityEngine;

namespace ProjectKai.Data
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speakerName;
        [TextArea(2, 5)]
        public string text;
        public Sprite portrait;
    }

    [CreateAssetMenu(fileName = "NewDialogue", menuName = "ProjectKai/Dialogue Data")]
    public class DialogueDataSO : ScriptableObject
    {
        public string dialogueId;
        public DialogueLine[] lines;
    }
}
