using UnityEngine;

public class SpaceShipState : MonoBehaviour
{
    protected SpaceShipController spaceShip;
    protected SpaceShipStateMachine stateMachine;

    public SpaceShipState(SpaceShipController spaceShip, SpaceShipStateMachine stateMachine) {
        this.spaceShip = spaceShip;
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
