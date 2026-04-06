using UnityEngine;

namespace ProjectKai.Combat
{
    public enum BufferedInput
    {
        None,
        Attack,
        Shoot,
        Dash,
        Jump,
        WeaponSwitch
    }

    public class InputBuffer
    {
        private BufferedInput _bufferedInput = BufferedInput.None;
        private float _bufferTime;
        private float _bufferTimer;

        public InputBuffer(float bufferTime = 0.15f)
        {
            _bufferTime = bufferTime;
        }

        public void Buffer(BufferedInput input)
        {
            _bufferedInput = input;
            _bufferTimer = _bufferTime;
        }

        public void Update()
        {
            if (_bufferTimer > 0f)
            {
                _bufferTimer -= Time.unscaledDeltaTime;
                if (_bufferTimer <= 0f)
                {
                    _bufferedInput = BufferedInput.None;
                }
            }
        }

        public BufferedInput Consume()
        {
            var input = _bufferedInput;
            _bufferedInput = BufferedInput.None;
            _bufferTimer = 0f;
            return input;
        }

        public bool HasInput(BufferedInput type)
        {
            return _bufferedInput == type;
        }

        public bool HasAnyInput()
        {
            return _bufferedInput != BufferedInput.None;
        }

        public void Clear()
        {
            _bufferedInput = BufferedInput.None;
            _bufferTimer = 0f;
        }
    }
}
