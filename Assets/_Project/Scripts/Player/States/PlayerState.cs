using ProjectKai.StateMachine;

namespace ProjectKai.Player.States
{
    /// <summary>
    /// 모든 플레이어 상태의 기본 클래스.
    /// 공통 로직(대시 체크, 피격 체크 등)을 여기서 처리.
    /// </summary>
    public abstract class PlayerState : IState
    {
        protected PlayerController Player;
        protected float StateTimer;

        protected PlayerState(PlayerController player)
        {
            Player = player;
        }

        public virtual void Enter()
        {
            StateTimer = 0f;
        }

        public virtual void Execute()
        {
            StateTimer += UnityEngine.Time.deltaTime;
        }

        public virtual void FixedExecute() { }

        public virtual void Exit() { }

        /// <summary>
        /// 수평 이동 처리 (가속/감속 적용)
        /// </summary>
        protected void HandleHorizontalMovement()
        {
            float targetSpeed = Player.Input.MoveInput.x * Player.MoveSpeed;
            float accel = (Player.GroundCheck != null && Player.GroundCheck.IsGrounded)
                ? Player.Acceleration : Player.AirAcceleration;
            float speedDiff = targetSpeed - Player.Rb.linearVelocity.x;
            float movement = speedDiff * accel * UnityEngine.Time.fixedDeltaTime;
            Player.Rb.AddForce(UnityEngine.Vector2.right * movement, UnityEngine.ForceMode2D.Force);
        }

        /// <summary>
        /// 대시 가능 여부를 확인하고 전환
        /// </summary>
        protected bool TryDash()
        {
            if (Player.InputBuffer.HasInput(Combat.BufferedInput.Dash) && Player.CanDash())
            {
                Player.InputBuffer.Consume();
                Player.StateMachine.ChangeState(Player.DashState);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 공격 입력 확인 (근접 or 원거리)
        /// </summary>
        protected bool TryAttack()
        {
            if (Player.InputBuffer.HasInput(Combat.BufferedInput.Attack))
            {
                Player.InputBuffer.Consume();
                Player.StateMachine.ChangeState(Player.MeleeAttackState);
                return true;
            }
            if (Player.InputBuffer.HasInput(Combat.BufferedInput.Shoot))
            {
                Player.InputBuffer.Consume();
                Player.StateMachine.ChangeState(Player.RangedAttackState);
                return true;
            }
            return false;
        }
    }
}
