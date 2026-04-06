using UnityEngine;
using ProjectKai.Combat;

namespace ProjectKai.Player.States
{
    public class FallState : PlayerState
    {
        public FallState(PlayerController player) : base(player) { }

        public override void Enter()
        {
            base.Enter();
            Player.SetGravityScale(Player.Rb.gravityScale * Player.FallGravityMultiplier);
            Player.SpriteAnim?.Play("fall");
        }

        public override void Execute()
        {
            base.Execute();

            if (TryDash()) return;

            // 코요테 타임 점프
            if (Player.InputBuffer.HasInput(BufferedInput.Jump) && Player.GroundCheck.IsGrounded)
            {
                Player.InputBuffer.Consume();
                Player.ResetGravity();
                Player.StateMachine.ChangeState(Player.JumpState);
                return;
            }

            // 에어 점프
            if (Player.InputBuffer.HasInput(BufferedInput.Jump) && Player.AirJumpsRemaining > 0)
            {
                Player.InputBuffer.Consume();
                Player.AirJumpsRemaining--;
                Player.ResetGravity();
                Player.StateMachine.ChangeState(Player.JumpState);
                return;
            }

            // 착지
            if (Player.GroundCheck.IsGroundedRaw && Player.Rb.linearVelocity.y <= 0f)
            {
                Player.ResetGravity();

                if (Mathf.Abs(Player.Input.MoveInput.x) > 0.1f)
                    Player.StateMachine.ChangeState(Player.RunState);
                else
                    Player.StateMachine.ChangeState(Player.IdleState);
                return;
            }
        }

        public override void FixedExecute()
        {
            HandleHorizontalMovement();
        }

        public override void Exit()
        {
            base.Exit();
            Player.ResetGravity();
        }
    }
}
