using ProjectKai.Combat;

namespace ProjectKai.Player.States
{
    public class RunState : PlayerState
    {
        public RunState(PlayerController player) : base(player) { }

        public override void Enter()
        {
            base.Enter();
            Player.AirJumpsRemaining = Player.MaxAirJumps;
            Player.SpriteAnim?.ResetToIdle();
            Player.SpriteAnim?.Play("run", 10f);
        }

        public override void Execute()
        {
            base.Execute();

            if (TryDash()) return;
            if (TryAttack()) return;

            if (Player.InputBuffer.HasInput(BufferedInput.Jump) && Player.GroundCheck.IsGrounded)
            {
                Player.InputBuffer.Consume();
                Player.StateMachine.ChangeState(Player.JumpState);
                return;
            }

            if (!Player.GroundCheck.IsGrounded)
            {
                Player.StateMachine.ChangeState(Player.FallState);
                return;
            }

            if (UnityEngine.Mathf.Abs(Player.Input.MoveInput.x) < 0.1f)
            {
                Player.StateMachine.ChangeState(Player.IdleState);
                return;
            }
        }

        public override void FixedExecute()
        {
            HandleHorizontalMovement();
        }
    }
}
