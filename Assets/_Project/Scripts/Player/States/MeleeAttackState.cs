using UnityEngine;
using ProjectKai.Combat;
using ProjectKai.Data;

namespace ProjectKai.Player.States
{
    public class MeleeAttackState : PlayerState
    {
        private ComboSystem _comboSystem;
        private DamageDealer _damageDealer;
        private ComboStep _currentStep;
        private bool _hasHit;
        private LayerMask _enemyLayer;

        public MeleeAttackState(PlayerController player, ComboSystem comboSystem,
            DamageDealer damageDealer, LayerMask enemyLayer) : base(player)
        {
            _comboSystem = comboSystem;
            _damageDealer = damageDealer;
            _enemyLayer = enemyLayer;
        }

        public override void Enter()
        {
            base.Enter();

            _currentStep = _comboSystem.AdvanceCombo();
            if (_currentStep == null)
            {
                Player.StateMachine.ChangeState(Player.IdleState);
                return;
            }

            _hasHit = false;

            // 공격 시 약간 전진
            if (_currentStep.moveForwardForce > 0f)
            {
                Player.SetVelocityX(Player.FacingDirection * _currentStep.moveForwardForce);
            }
            else
            {
                Player.SetVelocityX(0f);
            }

            // 콤보 단계별 애니메이션 + SFX
            int step = _comboSystem.CurrentStep;
            string animName = step switch
            {
                0 => "attack",
                1 => "attack2",
                _ => "attack3"
            };
            Player.SpriteAnim?.ForcePlay(animName);

            // SFX 피치 변화로 콤보 느낌 (단계 올라갈수록 높은 피치)
            float pitch = 1f + step * 0.15f;
            Core.AudioManager.Instance?.PlaySFX("sword_swing", 0.5f + step * 0.1f);

            // 스탯+무기 강화 데미지 반영
            float bonusDmg = Core.WeaponUpgrade.SwordDamageBonus;
            if (Core.ProgressionSystem.Instance != null)
                bonusDmg += Core.ProgressionSystem.Instance.BonusDamage;

            _damageDealer.Activate(
                _currentStep.damage + bonusDmg,
                _currentStep.knockbackForce,
                new Vector2(Player.FacingDirection, 0f),
                _enemyLayer
            );
        }

        public override void Execute()
        {
            base.Execute();

            // 히트 판정 타이밍
            if (!_hasHit && StateTimer >= _currentStep.hitStartTime && StateTimer <= _currentStep.hitEndTime)
            {
                Vector2 hitOrigin = (Vector2)Player.transform.position +
                    new Vector2(_currentStep.hitboxOffset.x * Player.FacingDirection, _currentStep.hitboxOffset.y);

                int hits = _damageDealer.PerformHitCheck(hitOrigin, _currentStep.hitboxSize);
                _hasHit = true;

                // 콤보 단계별 이펙트 강도
                if (hits > 0)
                {
                    int step = _comboSystem.CurrentStep;

                    // VFX: 콤보 단계별 슬래시 이펙트
                    Core.VFXManager.Instance?.SlashEffect(hitOrigin, step, Player.FacingDirection);
                    Core.VFXManager.Instance?.HitEffect(hitOrigin);

                    if (step >= 2) // 3타 (강타)
                    {
                        Core.GameFeel.Instance?.CameraShake(0.15f, 0.15f);
                        Core.GameFeel.Instance?.HitStop(0.08f);
                        Core.GameFeel.Instance?.CameraZoom(4.5f, 0.3f);
                    }
                    else if (step == 1) // 2타
                    {
                        Core.GameFeel.Instance?.CameraShake(0.1f, 0.1f);
                        Core.GameFeel.Instance?.HitStop(0.05f);
                    }
                    // 1타는 DamageReceiver에서 기본 처리
                }
            }

            // 공격 모션 완료
            if (StateTimer >= _currentStep.duration)
            {
                _comboSystem.AllowAdvance();

                // 버퍼에 다음 공격 입력이 있으면 연속 콤보 (상태머신 정상 전환)
                if (Player.InputBuffer.HasInput(BufferedInput.Attack))
                {
                    Player.InputBuffer.Consume();
                    Player.StateMachine.ForceChangeState(Player.MeleeAttackState);
                    return;
                }

                ReturnToMovementState();
                return;
            }
        }

        public override void Exit()
        {
            base.Exit();
            _damageDealer.Deactivate();
        }

        private void ReturnToMovementState()
        {
            if (!Player.GroundCheck.IsGrounded)
                Player.StateMachine.ChangeState(Player.FallState);
            else if (Mathf.Abs(Player.Input.MoveInput.x) > 0.1f)
                Player.StateMachine.ChangeState(Player.RunState);
            else
                Player.StateMachine.ChangeState(Player.IdleState);
        }
    }
}
