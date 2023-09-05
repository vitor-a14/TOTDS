using UnityEngine;

public class SpaceshipOnSpace : SpaceshipState
{
    public SpaceshipOnSpace(SpaceshipController spaceship, SpaceshipStateMachine stateMachine) : base(spaceship, stateMachine) { }

    public override void Interact() {
        spaceship.playerTeleportPoint = spaceship.FindPlayerTeleportPoint();
        if(spaceship.playerTeleportPoint == Vector3.zero) {
            Debug.Log("Non valid teleport point was found.");
            return;
        }

        spaceship.StateMachine.ChangeState(spaceship.IdleState);
    }

    public override void StateUpdate() { } 

    public override void StateFixedUpdate() { 
        spaceship.AvoidCollisions();
    } 

    public override void Boost() {

    }
}
