using UnityEngine;
using ProjectKai.Data;
using ProjectKai.Combat;

namespace ProjectKai.Player.States
{
    public class RangedAttackState : PlayerState
    {
        private WeaponDataSO _weaponData;
        private Transform _firePoint;
        private float _attackDuration;

        public RangedAttackState(PlayerController player, Transform firePoint) : base(player)
        {
            _firePoint = firePoint;
        }

        public void SetWeaponData(WeaponDataSO data)
        {
            _weaponData = data;
            _attackDuration = data != null ? data.fireRate : 0.2f;
        }

        public override void Enter()
        {
            base.Enter();

            if (_weaponData == null)
            {
                Player.StateMachine.ChangeState(Player.IdleState);
                return;
            }

            Fire();
        }

        public override void Execute()
        {
            base.Execute();

            if (StateTimer >= _attackDuration)
            {
                // 버퍼에 추가 발사 입력이 있으면 연사
                if (Player.InputBuffer.HasInput(Combat.BufferedInput.Shoot))
                {
                    Player.InputBuffer.Consume();
                    StateTimer = 0f;
                    Fire();
                    return;
                }

                ReturnToMovementState();
                return;
            }
        }

        private void Fire()
        {
            if (_weaponData.projectilePrefab != null && _firePoint != null)
            {
                var projectile = Object.Instantiate(
                    _weaponData.projectilePrefab,
                    _firePoint.position,
                    Quaternion.identity
                );

                var proj = projectile.GetComponent<Projectile>();
                if (proj != null)
                {
                    float bonusDmg = Core.WeaponUpgrade.GunDamageBonus;
                    if (Core.ProgressionSystem.Instance != null)
                        bonusDmg += Core.ProgressionSystem.Instance.BonusDamage * 0.5f;

                    proj.Initialize(
                        new Vector2(Player.FacingDirection, 0f),
                        _weaponData.projectileSpeed,
                        _weaponData.baseDamage + bonusDmg,
                        _weaponData.knockbackForce
                    );
                }
            }
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
