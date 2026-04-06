namespace ProjectKai.StateMachine
{
    public interface IState
    {
        void Enter();
        void Execute();
        void FixedExecute();
        void Exit();
    }
}
