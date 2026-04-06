using UnityEngine;
using ProjectKai.Combat;

namespace ProjectKai.Core
{
    /// <summary>
    /// 적 사망 시 아이템 드롭. HP 포션 또는 골드.
    /// DamageReceiver.OnDeath에 연결.
    /// </summary>
    public class ItemDrop : MonoBehaviour
    {
        [SerializeField] private float _dropChance = 0.3f;

        private void Start()
        {
            var dr = GetComponent<DamageReceiver>();
            if (dr != null)
                dr.OnDeath += OnDeath;
        }

        private void OnDeath()
        {
            if (Random.value > _dropChance) return;

            // HP 포션 드롭
            var pickupObj = new GameObject("DroppedPotion");
            pickupObj.transform.position = transform.position + new Vector3(0, 0.5f, 0);

            var sr = pickupObj.AddComponent<SpriteRenderer>();
            sr.color = new Color(1f, 0.3f, 0.3f);
            sr.sortingOrder = 3;

            var tex = new Texture2D(4, 4);
            var pix = new Color[16];
            for (int i = 0; i < 16; i++) pix[i] = Color.white;
            tex.SetPixels(pix); tex.filterMode = FilterMode.Point; tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 16f);

            pickupObj.transform.localScale = new Vector3(0.4f, 0.5f, 1f);

            var rb = pickupObj.AddComponent<Rigidbody2D>();
            rb.gravityScale = 2f;
            rb.linearVelocity = new Vector2(Random.Range(-2f, 2f), 4f);

            var col = pickupObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 1f);

            pickupObj.AddComponent<HealthPickup>();

            Destroy(pickupObj, 10f);
        }
    }
}
