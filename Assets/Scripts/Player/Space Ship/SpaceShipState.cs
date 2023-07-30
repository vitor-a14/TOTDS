using UnityEngine;

public class SpaceShipState : MonoBehaviour
{
    protected SpaceShipController spaceShip;
    protected SpaceShipStateMachine stateMachine;

    public SpaceShipState(SpaceShipController spaceShip, SpaceShipStateMachine stateMachine) {
        this.spaceShip = spaceShip;
        this.stateMachine = stateMachine;
    }

    //States

    public virtual void Enter() { }

    public virtual void Exit() { }

    public virtual void StateUpdate() { }

    public virtual void StateFixedUpdate() { }


    //Inputs

    public virtual void Interact() { }
}
