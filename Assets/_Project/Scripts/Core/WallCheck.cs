using UnityEngine;

namespace ProjectKai.Core
{
    /// <summary>
    /// 벽 감지. 플레이어 좌우에 레이캐스트.
    /// 벽 점프 시스템의 기반.
    /// </summary>
    public class WallCheck : MonoBehaviour
    {
        [SerializeField] private float _rayLength = 0.3f;
        [SerializeField] private LayerMask _wallLayer;

        public bool IsTouchingWallLeft { get; private set; }
        public bool IsTouchingWallRight { get; private set; }
        public bool IsTouchingWall => IsTouchingWallLeft || IsTouchingWallRight;
        public int WallDirection => IsTouchingWallLeft ? -1 : (IsTouchingWallRight ? 1 : 0);

        private Collider2D _parentCollider;

        private void Awake()
        {
            _parentCollider = GetComponentInParent<Collider2D>();
            if (_wallLayer == 0)
                _wallLayer = ~0; // 모든 레이어
        }

        private void FixedUpdate()
        {
            Vector2 pos = transform.position;

            // 왼쪽 벽
            var hitLeft = Physics2D.Raycast(pos, Vector2.left, _rayLength, _wallLayer);
            IsTouchingWallLeft = hitLeft.collider != null && hitLeft.collider != _parentCollider;

            // 오른쪽 벽
            var hitRight = Physics2D.Raycast(pos, Vector2.right, _rayLength, _wallLayer);
            IsTouchingWallRight = hitRight.collider != null && hitRight.collider != _parentCollider;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsTouchingWallLeft ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.left * _rayLength);

            Gizmos.color = IsTouchingWallRight ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, transform.position + Vector3.right * _rayLength);
        }
    }
}
