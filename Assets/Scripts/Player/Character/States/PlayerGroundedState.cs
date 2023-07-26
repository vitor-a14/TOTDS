using UnityEngine;

public class PlayerGroundedState : PlayerState
{
    private Vector3 direction;
    private Vector3 processedDirection;

    private float durationOffGround;
    private float maxOffGroundDuration = 0.16f;

    private float stepTimer = 0f;
    private float stepDuration = 0.3f;

    public PlayerGroundedState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter() { 
        player.processedInput = Vector2.zero;
        durationOffGround = 0f;
    }

    public override void StateUpdate() { 
        Vector3 gravityDirection = player.GetGravityDirection();
        Vector3 forward = Vector3.Cross(-gravityDirection, player.cam.right).normalized;
        Vector3 right = Vector3.Cross(-gravityDirection, -player.cam.forward).normalized;
        float movementMultiplier = 1;

        HandleSlopes();

        if (player.onSlope)
            movementMultiplier = player.movementSpeedOnSlope;
        else 
            movementMultiplier = player.movementSpeed;

        //Calculate movement direction
        direction = (forward * player.processedInput.y + right * player.processedInput.x) * movementMultiplier;

        //Rotate player model
        if (direction != Vector3.zero && player.input != Vector2.zero) {
            Quaternion modelRotation = Quaternion.LookRotation(direction.normalized, gravityDirection);
            player.characterModel.rotation = Quaternion.Slerp(player.characterModel.rotation, modelRotation, 15f * Time.deltaTime);
        } else {
            Vector3 forwardDir = Vector3.Cross(gravityDirection, player.characterModel.right);
            Quaternion modelRotation = Quaternion.LookRotation(-forwardDir, gravityDirection);
            player.characterModel.rotation = Quaternion.Slerp(player.characterModel.rotation, modelRotation, 15f * Time.deltaTime);
        }

        //Start jump
        if(player.jumpButtonIsPressed && player.onGround) {
            stateMachine.ChangeState(player.JumpState);
        }

        //Check if the player is falling
        if(!player.onGround) {
            durationOffGround += Time.deltaTime;
            if(durationOffGround > maxOffGroundDuration) {
                player.StateMachine.ChangeState(player.FallState);
            } 
        } else {
            if(durationOffGround > 0f) {
                stepTimer = 0f;
            }
            durationOffGround = 0f;
        }
    }

    public override void StateFixedUpdate() {
        StepUp();
            
        if(!player.nearWall) {
            player.rigid.position += direction * Time.fixedDeltaTime;
        }
    }

    private void HandleSlopes() {
        //Check if the player need to strumble
        if(stepTimer < stepDuration) {
            stepTimer += Time.deltaTime;
            player.onSlope = true;
        } else {
            player.onSlope = Vector3.Dot(player.transform.up, player.surfaceNormal) < player.slopeAngleTrigger ? true : false; 
        }
    }

    private void StepUp() {
        bool stepDetected = true;
        float heightOffset = 0;
        Transform characterModel = player.characterModel;

        if(Physics.Raycast(characterModel.position + characterModel.up * 0.08f, characterModel.forward, out RaycastHit frontHit, player.characterCollider.radius + 0.05f, player.walkableLayers)) {
            float stepness = Vector3.Dot(characterModel.up, frontHit.normal);
            if(stepness > 0.2f) 
                stepDetected = false;
        } else {
            stepDetected = false;
        }

        Vector3 startPos = characterModel.position + characterModel.forward * (player.characterCollider.radius + 0.05f) + characterModel.up;
        if(Physics.Raycast(startPos, -characterModel.up, out RaycastHit downHit, 2.5f, player.walkableLayers)) {
            heightOffset = Vector3.Distance(downHit.point, characterModel.position + characterModel.forward * (player.characterCollider.radius + 0.05f));
            if(heightOffset > player.maxStepHeight)
                stepDetected = false;
        } else {
            stepDetected = false;
        }

        if(stepDetected) {
            stepTimer = 0f;
            player.rigid.position += characterModel.up * (heightOffset + 0.02f) + characterModel.forward * 0.067f;
        }
    }

    public override void Exit() { }
}
