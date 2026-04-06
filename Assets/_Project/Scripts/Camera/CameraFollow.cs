using UnityEngine;

namespace ProjectKai.Camera
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Follow Settings")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 1f, -10f);
        [SerializeField] private float _smoothSpeed = 8f;

        [Header("Look Ahead")]
        [SerializeField] private float _lookAheadDistance = 2f;
        [SerializeField] private float _lookAheadSpeed = 3f;

        [Header("Bounds (Optional)")]
        [SerializeField] private bool _useBounds;
        [SerializeField] private Vector2 _boundsMin;
        [SerializeField] private Vector2 _boundsMax;

        private float _currentLookAhead;

        private void Awake()
        {
            if (_target == null)
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null) _target = player.transform;
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            // Look Ahead 계산
            float targetLookAhead = 0f;
            var rb = _target.GetComponent<Rigidbody2D>();
            if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.5f)
            {
                targetLookAhead = Mathf.Sign(rb.linearVelocity.x) * _lookAheadDistance;
            }
            _currentLookAhead = Mathf.Lerp(_currentLookAhead, targetLookAhead, _lookAheadSpeed * Time.deltaTime);

            Vector3 targetPos = _target.position + _offset;
            targetPos.x += _currentLookAhead;

            // 바운드 제한
            if (_useBounds)
            {
                targetPos.x = Mathf.Clamp(targetPos.x, _boundsMin.x, _boundsMax.x);
                targetPos.y = Mathf.Clamp(targetPos.y, _boundsMin.y, _boundsMax.y);
            }

            transform.position = Vector3.Lerp(transform.position, targetPos, _smoothSpeed * Time.deltaTime);
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            _useBounds = true;
            _boundsMin = min;
            _boundsMax = max;
        }
    }
}
