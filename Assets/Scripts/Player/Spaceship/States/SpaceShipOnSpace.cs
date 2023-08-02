using UnityEngine;

public class SpaceShipOnSpace : SpaceShipState
{
    public SpaceShipOnSpace(SpaceShipController spaceShip, SpaceShipStateMachine stateMachine) : base(spaceShip, stateMachine) { }

    public override void Interact() {
        spaceShip.playerTeleportPoint = spaceShip.FindPlayerTeleportPoint();
        if(spaceShip.playerTeleportPoint == Vector3.zero) {
            Debug.Log("Non valid teleport point was found.");
            return;
        }

        spaceShip.StateMachine.ChangeState(spaceShip.IdleState);
    }

    public override void StateUpdate() { } 

    public override void StateFixedUpdate() { 
        spaceShip.AvoidCollisions();
    } 

    public override void Boost() {

    }
}
