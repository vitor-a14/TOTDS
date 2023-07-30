public class SpaceShipStateMachine
{
    public SpaceShipState CurrentState { get; private set; }

    public void Initialize(SpaceShipState startingState) { 
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(SpaceShipState newState) {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }
}
