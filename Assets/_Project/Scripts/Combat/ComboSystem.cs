using UnityEngine;
using ProjectKai.Data;

namespace ProjectKai.Combat
{
    public class ComboSystem
    {
        private ComboDataSO _comboData;
        private int _currentStep;
        private float _comboTimer;
        private bool _canAdvance;

        public int CurrentStep => _currentStep;
        public ComboStep CurrentComboStep => _comboData != null && _currentStep < _comboData.steps.Length
            ? _comboData.steps[_currentStep]
            : null;
        public bool IsComboActive => _comboTimer > 0f;

        public void SetComboData(ComboDataSO data)
        {
            _comboData = data;
            Reset();
        }

        public ComboStep AdvanceCombo()
        {
            if (_comboData == null || _comboData.steps.Length == 0) return null;

            if (!_canAdvance && _currentStep > 0)
                return null;

            if (_currentStep >= _comboData.steps.Length)
            {
                _currentStep = 0;
            }

            var step = _comboData.steps[_currentStep];
            _comboTimer = _comboData.comboResetTime;
            _canAdvance = false;

            return step;
        }

        /// <summary>
        /// 현재 공격 모션이 끝나면 호출하여 다음 단계 진행 허용
        /// </summary>
        public void AllowAdvance()
        {
            _canAdvance = true;
            _currentStep++;
            if (_currentStep >= _comboData.steps.Length)
            {
                _currentStep = 0;
            }
        }

        public void Update()
        {
            if (_comboTimer > 0f)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f)
                {
                    Reset();
                }
            }
        }

        public void Reset()
        {
            _currentStep = 0;
            _comboTimer = 0f;
            _canAdvance = true;
        }
    }
}
