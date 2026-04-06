using UnityEngine;

namespace ProjectKai.Core
{
    /// <summary>
    /// 패럴랙스 배경 레이어. 카메라 이동에 따라 느리게 따라옴.
    /// parallaxFactor: 0 = 고정, 1 = 카메라와 동일 속도
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private float _parallaxFactor = 0.5f;

        private Transform _cam;
        private Vector3 _startPos;
        private float _startCamX;

        private void Start()
        {
            _cam = UnityEngine.Camera.main?.transform;
            _startPos = transform.position;
            if (_cam != null)
                _startCamX = _cam.position.x;
        }

        private void LateUpdate()
        {
            if (_cam == null) return;

            float deltaX = _cam.position.x - _startCamX;
            transform.position = new Vector3(
                _startPos.x + deltaX * _parallaxFactor,
                _startPos.y,
                _startPos.z);
        }
    }
}
