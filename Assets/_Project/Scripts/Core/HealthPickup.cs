using UnityEngine;
using ProjectKai.Player;

namespace ProjectKai.Core
{
    /// <summary>
    /// HP 회복 아이템. 접촉 시 체력 회복 + 사라짐.
    /// DungeonTilesetII의 flask_red 스프라이트 사용.
    /// </summary>
    public class HealthPickup : MonoBehaviour
    {
        [SerializeField] private float _healAmount = 30f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null && player.IsAlive)
            {
                player.Heal(_healAmount);
                AudioManager.Instance?.PlaySFX("jump", 0.6f);
                GameFeel.Instance?.CameraShake(0.03f, 0.05f);
                Debug.Log($"[HealthPickup] +{_healAmount} HP");
                Destroy(gameObject);
            }
        }
    }
}
