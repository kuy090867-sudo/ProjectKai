using UnityEngine;

namespace ProjectKai.Combat
{
    public enum StatusType { Poison, Burn, Freeze, Stun }

    [System.Serializable]
    public class StatusEffect
    {
        public StatusType type;
        public float duration;
        public float damagePerTick;
        public float tickInterval;
        public float speedMultiplier = 1f;
    }
}
