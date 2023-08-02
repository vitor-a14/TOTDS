using UnityEngine;

public class SpaceShipOnPlanet : SpaceShipState
{
    private float fowardVelocity;
    private float sidesVelocity;

    private Vector3 processedDirection;
    private Vector3 direction;
    private Vector3 torque;

    public SpaceShipOnPlanet(SpaceShipController spaceShip, SpaceShipStateMachine stateMachine) : base(spaceShip, stateMachine) { }

    public override void Interact() {
        spaceShip.playerTeleportPoint = spaceShip.FindPlayerTeleportPoint();
        if(spaceShip.playerTeleportPoint == Vector3.zero) {
            Debug.Log("Non valid teleport point was found.");
            return;
        }

        spaceShip.StateMachine.ChangeState(spaceShip.IdleState);
    }

    public override void StateUpdate() { 
        torque = spaceShip.transform.TransformDirection(-spaceShip.rotationInput.y, spaceShip.rotationInput.x, -spaceShip.inputRoll * 0.5f);
        direction = spaceShip.transform.TransformDirection(spaceShip.movementInput.x * 0.5f, spaceShip.inputAltitude * 0.5f, spaceShip.movementInput.y);
        processedDirection = Vector3.Lerp(processedDirection, direction, spaceShip.acceleration * Time.deltaTime);
    } 

    public override void StateFixedUpdate() {
        spaceShip.rigid.AddForce(processedDirection * spaceShip.maxSpeed, ForceMode.Acceleration);
        spaceShip.rigid.AddTorque(torque * spaceShip.torque * 10f);
        spaceShip.AvoidCollisions();
    }

    public override void Boost() { }
}
