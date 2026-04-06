using UnityEngine;

namespace ProjectKai.Data
{
    [System.Serializable]
    public class ComboStep
    {
        public string animationTrigger;
        public float damage;
        public float duration;
        public float hitStartTime;
        public float hitEndTime;
        public float knockbackForce = 5f;
        public Vector2 hitboxOffset = new Vector2(1f, 0f);
        public Vector2 hitboxSize = new Vector2(1f, 1f);
        public float moveForwardForce;
    }

    [CreateAssetMenu(fileName = "NewCombo", menuName = "ProjectKai/Combo Data")]
    public class ComboDataSO : ScriptableObject
    {
        public string comboName;
        public ComboStep[] steps;
        public float comboResetTime = 0.6f;
    }
}
