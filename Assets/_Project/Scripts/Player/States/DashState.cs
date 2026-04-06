using UnityEngine;

namespace ProjectKai.Player.States
{
    public class DashState : PlayerState
    {
        public DashState(PlayerController player) : base(player) { }

        public override void Enter()
        {
            base.Enter();
            Player.LastDashTime = Time.time;
            Player.SetGravityScale(0f);
            Player.SetVelocity(Player.FacingDirection * Player.DashSpeed, 0f);
            Player.SpriteAnim?.Play("dash");
            Core.AudioManager.Instance?.PlaySFX("dash", 0.4f);

            // VFX: 대시 시작 먼지
            Core.VFXManager.Instance?.DustPuff(Player.transform.position);

            // 대시 무적 + 트레일
            Player.IsInvincible = true;
            var trail = Player.GetComponentInChildren<TrailRenderer>();
            if (trail != null) trail.emitting = true;
        }

        public override void Execute()
        {
            base.Execute();

            if (StateTimer >= Player.DashDuration)
            {
                EndDash();
                return;
            }
        }

        public override void Exit()
        {
            base.Exit();
            Player.ResetGravity();
            Player.IsInvincible = false;

            var trail = Player.GetComponentInChildren<TrailRenderer>();
            if (trail != null) trail.emitting = false;
        }

        private void EndDash()
        {
            Player.SetVelocityX(Player.FacingDirection * Player.MoveSpeed);

            if (Player.GroundCheck.IsGrounded)
            {
                if (Mathf.Abs(Player.Input.MoveInput.x) > 0.1f)
                    Player.StateMachine.ChangeState(Player.RunState);
                else
                    Player.StateMachine.ChangeState(Player.IdleState);
            }
            else
            {
                Player.StateMachine.ChangeState(Player.FallState);
            }
        }
    }
}
