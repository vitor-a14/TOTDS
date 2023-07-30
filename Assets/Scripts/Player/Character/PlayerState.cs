public abstract class PlayerState 
{
    protected PlayerController player;
    protected PlayerStateMachine stateMachine;

    public PlayerState(PlayerController player, PlayerStateMachine stateMachine) {
        this.player = player;
        this.stateMachine = stateMachine;
    }

    //State functions

    public virtual void Enter() { }

    public virtual void Exit() { }

    public virtual void StateUpdate() { }

    public virtual void StateFixedUpdate() { }


    //Input functions

    public virtual void Interact() { }

    public virtual void Jump() { }
} 
