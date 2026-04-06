using UnityEngine;
using ProjectKai.Combat;

namespace ProjectKai.Player.States
{
    public class JumpState : PlayerState
    {
        public JumpState(PlayerController player) : base(player) { }

        public override void Enter()
        {
            base.Enter();
            Player.SetVelocityY(Player.JumpForce);
            Player.GroundCheck.ConsumeCoyote();
            Player.SpriteAnim?.Play("jump");
            Core.AudioManager.Instance?.PlaySFX("jump", 0.5f);
        }

        public override void Execute()
        {
            base.Execute();

            if (TryDash()) return;
            if (TryAttack()) return;

            // 점프 버튼을 놓으면 점프 높이 제한 (숏 점프)
            if (!Player.Input.JumpHeld && Player.Rb.linearVelocity.y > 0f)
            {
                Player.SetVelocityY(Player.Rb.linearVelocity.y * Player.JumpCutMultiplier);
                Player.StateMachine.ChangeState(Player.FallState);
                return;
            }

            // 에어 점프
            if (Player.InputBuffer.HasInput(BufferedInput.Jump) && Player.AirJumpsRemaining > 0)
            {
                Player.InputBuffer.Consume();
                Player.AirJumpsRemaining--;
                Player.SetVelocityY(Player.JumpForce);
                StateTimer = 0f;
                return;
            }

            // 낙하 전환
            if (Player.Rb.linearVelocity.y <= 0f)
            {
                Player.StateMachine.ChangeState(Player.FallState);
                return;
            }
        }

        public override void FixedExecute()
        {
            HandleHorizontalMovement();
        }
    }
}
