using UnityEngine;

public class SpaceshipState : MonoBehaviour
{
    protected SpaceshipController spaceship;
    protected SpaceshipStateMachine stateMachine;

    public SpaceshipState(SpaceshipController spaceship, SpaceshipStateMachine stateMachine) {
        this.spaceship = spaceship;
        this.stateMachine = stateMachine;
    }

    //Default states

    public virtual void Enter() { }

    public virtual void Exit() { }

    public virtual void StateUpdate() { }

    public virtual void StateFixedUpdate() { }


    //Inputs from input system

    public virtual void Interact() { }

    public virtual void Boost() { }
}
