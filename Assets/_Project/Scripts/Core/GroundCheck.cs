using UnityEngine;

namespace ProjectKai.Core
{
    public class GroundCheck : MonoBehaviour
    {
        [Header("Ground Detection")]
        [SerializeField] private float _rayLength = 0.1f;
        [SerializeField] private float _raySpacing = 0.15f;

        [Header("Coyote Time")]
        [SerializeField] private float _coyoteTime = 0.1f;

        private float _coyoteTimer;
        private bool _isGroundedRaw;
        private Collider2D _parentCollider;

        public bool IsGrounded => _isGroundedRaw || _coyoteTimer > 0f;
        public bool IsGroundedRaw => _isGroundedRaw;

        private void Awake()
        {
            _parentCollider = GetComponentInParent<Collider2D>();
        }

        private void FixedUpdate()
        {
            _isGroundedRaw = CheckGround();

            if (_isGroundedRaw)
            {
                _coyoteTimer = _coyoteTime;
            }
            else
            {
                _coyoteTimer -= Time.fixedDeltaTime;
            }
        }

        private bool CheckGround()
        {
            Vector2 origin = transform.position;

            // 3개의 레이캐스트 (왼쪽, 중앙, 오른쪽)
            for (int i = -1; i <= 1; i++)
            {
                Vector2 rayOrigin = origin + Vector2.right * (i * _raySpacing);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, _rayLength);

                if (hit.collider != null && hit.collider != _parentCollider)
                {
                    return true;
                }
            }
            return false;
        }

        public void ConsumeCoyote()
        {
            _coyoteTimer = 0f;
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 origin = transform.position;
            for (int i = -1; i <= 1; i++)
            {
                Vector2 rayOrigin = origin + Vector2.right * (i * _raySpacing);
                Gizmos.color = _isGroundedRaw ? Color.green : Color.red;
                Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * _rayLength);
            }
        }
    }
}
