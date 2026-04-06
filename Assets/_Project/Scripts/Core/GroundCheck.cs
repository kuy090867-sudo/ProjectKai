using UnityEngine;

namespace ProjectKai.Core
{
    public class GroundCheck : MonoBehaviour
    {
        [Header("Ground Detection")]
        [SerializeField] private float _rayLength = 0.3f;
        [SerializeField] private float _raySpacing = 0.2f;
        [SerializeField] private Vector2 _boxSize = new Vector2(0.4f, 0.1f);

        [Header("Coyote Time")]
        [SerializeField] private float _coyoteTime = 0.12f;

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
                _coyoteTimer = _coyoteTime;
            else
                _coyoteTimer -= Time.fixedDeltaTime;
        }

        private bool CheckGround()
        {
            Vector2 origin = transform.position;

            // 방법 1: OverlapBox (가장 확실)
            var hits = Physics2D.OverlapBoxAll(origin + Vector2.down * 0.05f, _boxSize, 0f);
            foreach (var hit in hits)
            {
                if (hit != _parentCollider && !hit.isTrigger)
                    return true;
            }

            // 방법 2: Raycast 3개 (폴백)
            for (int i = -1; i <= 1; i++)
            {
                Vector2 rayOrigin = origin + Vector2.right * (i * _raySpacing);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, _rayLength);
                if (hit.collider != null && hit.collider != _parentCollider && !hit.collider.isTrigger)
                    return true;
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
            Gizmos.color = _isGroundedRaw ? Color.green : Color.red;
            Gizmos.DrawWireCube(origin + Vector2.down * 0.05f, _boxSize);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 rayOrigin = origin + Vector2.right * (i * _raySpacing);
                Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.down * _rayLength);
            }
        }
    }
}
