using UnityEngine;
using ProjectKai.Combat;
using ProjectKai.Core;

namespace ProjectKai.Player.States
{
    /// <summary>
    /// 벽 슬라이드 + 벽 점프.
    /// 공중에서 벽에 닿으면 천천히 미끄러지며, 점프로 벽 점프.
    /// </summary>
    public class WallSlideState : PlayerState
    {
        private WallCheck _wallCheck;

        public WallSlideState(PlayerController player) : base(player) { }

        public override void Enter()
        {
            base.Enter();
            _wallCheck = Player.GetComponentInChildren<WallCheck>();
            Player.SetGravityScale(Player.DefaultGravityScale * 0.3f); // 느린 낙하
            Player.SpriteAnim?.Play("fall");
        }

        public override void Execute()
        {
            base.Execute();

            // 벽에서 떨어지면 FallState
            if (_wallCheck == null || !_wallCheck.IsTouchingWall)
            {
                Player.ResetGravity();
                Player.StateMachine.ChangeState(Player.FallState);
                return;
            }

            // 착지하면 IdleState
            if (Player.GroundCheck.IsGroundedRaw)
            {
                Player.ResetGravity();
                Player.StateMachine.ChangeState(Player.IdleState);
                return;
            }

            // 벽 점프
            if (Player.InputBuffer.HasInput(BufferedInput.Jump))
            {
                Player.InputBuffer.Consume();
                Player.ResetGravity();

                // 벽 반대 방향으로 점프
                int jumpDir = -_wallCheck.WallDirection;
                Player.SetVelocity(jumpDir * Player.MoveSpeed * 1.2f, Player.JumpForce * 0.9f);

                // 방향 전환
                typeof(PlayerController).GetProperty("FacingDirection")
                    ?.SetValue(Player, jumpDir);

                Player.AirJumpsRemaining = Player.MaxAirJumps;
                AudioManager.Instance?.PlaySFX("jump", 0.6f);
                Player.StateMachine.ChangeState(Player.JumpState);
                return;
            }

            // 낙하 속도 제한
            if (Player.Rb.linearVelocity.y < -2f)
                Player.SetVelocityY(-2f);
        }

        public override void Exit()
        {
            base.Exit();
            Player.ResetGravity();
        }
    }
}
