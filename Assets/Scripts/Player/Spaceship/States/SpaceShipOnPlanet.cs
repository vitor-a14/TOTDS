using UnityEngine;

public class SpaceshipOnPlanet : SpaceshipState
{
    private Vector3 processedDirection;
    private Vector3 direction;
    private Vector3 torque;

    private bool boostMode = false;

    public SpaceshipOnPlanet(SpaceshipController spaceship, SpaceshipStateMachine stateMachine) : base(spaceship, stateMachine) { }

    public override void Interact() {
        spaceship.playerTeleportPoint = spaceship.FindPlayerTeleportPoint();
        if(spaceship.playerTeleportPoint == Vector3.zero) {
            Debug.Log("Non valid teleport point was found.");
            return;
        }

        spaceship.StateMachine.ChangeState(spaceship.IdleState);
    }

    public override void StateUpdate() { 
        float boostMultiplier = 1f;
        float boostFriction = 1f;

        if(boostMode) {
            if(spaceship.movementInput.y < 0.3f)
                boostMode = false;

            boostMultiplier = spaceship.boostMultiplier;
            boostFriction = spaceship.boostTorqueFriction;
        }

        torque = (Vector3.up * spaceship.rotationInput.x + Vector3.forward * -spaceship.inputRoll * 0.5f + Vector3.right * -spaceship.rotationInput.y) * boostFriction;
        direction = spaceship.transform.forward * spaceship.movementInput.y * boostMultiplier + (spaceship.transform.right * spaceship.movementInput.x + spaceship.transform.up * spaceship.inputAltitude) * 0.5f;
        processedDirection = Vector3.Lerp(processedDirection, direction, spaceship.acceleration * Time.deltaTime);

        spaceship.audioHandler.HandleEngineSound();
    } 

    public override void StateFixedUpdate() {
        spaceship.rigid.AddForce(processedDirection * spaceship.maxSpeed, ForceMode.Acceleration);
        spaceship.rigid.AddRelativeTorque(torque * spaceship.torque * 10f);
        spaceship.ProcessCameraMovement(true);
        spaceship.AvoidCollisions();
    }

    public override void Boost() { 
        boostMode = true;
    }
}
