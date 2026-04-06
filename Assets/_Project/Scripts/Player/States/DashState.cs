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

            // 대시 트레일 활성화
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

            // 대시 트레일 비활성화
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
