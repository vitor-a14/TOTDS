using System.Collections;
using UnityEngine;

public class PlayerJumpState : PlayerState
{
    private Vector3 direction;
    private bool canMovePlayer;  
    private MonoBehaviour monoBehaviour;
    private bool preparingJump;

    public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) {
        monoBehaviour = player.GetComponent<MonoBehaviour>();
    }

    public override void Enter() {
        monoBehaviour.StartCoroutine(HandleJumpCoroutine());
    }

    public override void StateUpdate() { 
        if(canMovePlayer) {
            Vector3 gravityDirection = player.GetGravityDirection();
            Vector3 forward = Vector3.Cross(-gravityDirection, player.cam.right).normalized;
            Vector3 right = Vector3.Cross(-gravityDirection, -player.cam.forward).normalized;
            direction = (forward * player.processedInput.y + right * player.processedInput.x) * player.movementSpeedOnAir;

            if (direction != Vector3.zero && player.input != Vector2.zero) {
                Quaternion modelRotation = Quaternion.LookRotation(direction.normalized, gravityDirection);
                player.characterModel.rotation = Quaternion.Slerp(player.characterModel.rotation, modelRotation, 15f * Time.deltaTime);
            } else {
                Vector3 forwardDir = Vector3.Cross(gravityDirection, player.characterModel.right);
                Quaternion modelRotation = Quaternion.LookRotation(-forwardDir, gravityDirection);
                player.characterModel.rotation = Quaternion.Slerp(player.characterModel.rotation, modelRotation, 15f * Time.deltaTime);
            }
        }

        if(canMovePlayer && player.onGround && !preparingJump) {
            monoBehaviour.StartCoroutine(HandleLandCoroutine());
        }
    }

    public override void StateFixedUpdate() { 
        if(canMovePlayer && !player.nearWall) {
            player.rigid.position += direction * Time.fixedDeltaTime;
        }
    }

    private IEnumerator HandleJumpCoroutine() {
        canMovePlayer = false;
        preparingJump = true;
        CharacterAnimation.Instance.PlayJumpAnim();

        yield return new WaitForSeconds(player.jumpIdleDuration);

        player.rigid.AddForce(player.surfaceNormal * player.jumpForce, ForceMode.VelocityChange);
        canMovePlayer = true;

        yield return new WaitForSeconds(0.1f);

        preparingJump = false;
    }  

    private IEnumerator HandleLandCoroutine() {
        canMovePlayer = false;
        CharacterAnimation.Instance.PlayLandAnim();

        yield return new WaitForSeconds(player.landIdleDuration);

        player.StateMachine.ChangeState(player.GroundedState);
    }

    public override void Exit() { }
}
