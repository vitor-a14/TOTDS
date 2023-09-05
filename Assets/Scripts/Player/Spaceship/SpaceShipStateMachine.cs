public class SpaceshipStateMachine
{
    public SpaceshipState CurrentState { get; private set; }

    public void Initialize(SpaceshipState startingState) { 
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(SpaceshipState newState) {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
