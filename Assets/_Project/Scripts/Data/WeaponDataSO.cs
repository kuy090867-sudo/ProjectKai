using UnityEngine;

namespace ProjectKai.Data
{
    public enum WeaponType
    {
        Melee,
        Ranged
    }

    [CreateAssetMenu(fileName = "NewWeapon", menuName = "ProjectKai/Weapon Data")]
    public class WeaponDataSO : ScriptableObject
    {
        [Header("Basic Info")]
        public string weaponName;
        public WeaponType weaponType;
        public Sprite icon;

        [Header("Stats")]
        public float baseDamage = 10f;
        public float attackSpeed = 1f;
        public float range = 1.5f;
        public float knockbackForce = 5f;

        [Header("Ranged Only")]
        public GameObject projectilePrefab;
        public float projectileSpeed = 15f;
        public float fireRate = 0.2f;
    }
}
