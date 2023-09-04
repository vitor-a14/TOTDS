using UnityEngine;

public class SpaceShipOnPlanet : SpaceShipState
{
    private Vector3 processedDirection;
    private Vector3 direction;
    private Vector3 torque;

    private bool boostMode = false;

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
        float boostMultiplier = 1f;
        float boostFriction = 1f;

        if(boostMode) {
            if(spaceShip.movementInput.y < 0.3f)
                boostMode = false;

            boostMultiplier = spaceShip.boostMultiplier;
            boostFriction = spaceShip.boostTorqueFriction;
        }

        torque = (Vector3.up * spaceShip.rotationInput.x + Vector3.forward * -spaceShip.inputRoll * 0.5f + Vector3.right * -spaceShip.rotationInput.y) * boostFriction;
        //direction = spaceShip.transform.TransformDirection(spaceShip.movementInput.x * 0.5f, spaceShip.inputAltitude * 0.5f, spaceShip.movementInput.y * boostMultiplier);
        direction = spaceShip.transform.forward * spaceShip.movementInput.y * boostMultiplier + (spaceShip.transform.right * spaceShip.movementInput.x + spaceShip.transform.up * spaceShip.inputAltitude) * 0.5f;
        processedDirection = Vector3.Lerp(processedDirection, direction, spaceShip.acceleration * Time.deltaTime);
    } 

    public override void StateFixedUpdate() {
        spaceShip.rigid.AddForce(processedDirection * spaceShip.maxSpeed, ForceMode.Acceleration);
        spaceShip.rigid.AddRelativeTorque(torque * spaceShip.torque * 10f);
        spaceShip.ProcessCameraMovement();
        spaceShip.AvoidCollisions();
    }

    public override void Boost() { 
        boostMode = true;
    }
}
