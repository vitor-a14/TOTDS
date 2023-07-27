using System.Collections;
using UnityEngine;

public class PlayerFallState : PlayerState
{
    private float fallingDuration;
    private bool handleFall;
    private MonoBehaviour monoBehaviour;

    private float faintDuration = 3.8f;

    public PlayerFallState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) {
        monoBehaviour = player.GetComponent<MonoBehaviour>();
    }

    public override void Enter() { 
        handleFall = true;
        fallingDuration = 0f;
        player.isFalling = true;
        CharacterAnimation.Instance.PlayFallAnim();
    }

    public override void Exit() { 
        player.isFalling = false;
    }

    public override void StateUpdate() {
        if(player.onGround && handleFall) {
            if(fallingDuration <= player.smallFallDuration) {
                monoBehaviour.StartCoroutine(HandleSmallLand());
            } else if(fallingDuration <= player.normalFallDuration) {
                monoBehaviour.StartCoroutine(HandleMediumLand());
            } else {
                monoBehaviour.StartCoroutine(HandleBigLand());
            }

            handleFall = false;
        } else {
            fallingDuration += Time.deltaTime;
        }
    }

    private IEnumerator HandleSmallLand() {
        if(player.input.sqrMagnitude > 0.3f) {
            CharacterAnimation.Instance.PlaySmallLandMoving();
            player.rigid.AddForce(player.characterModel.forward * player.jumpForce, ForceMode.VelocityChange);
        } else {
            CharacterAnimation.Instance.PlaySmallLandIdle();
        }
        yield return new WaitForSeconds(player.landIdleDuration/2);
        player.StateMachine.ChangeState(player.GroundedState);
    }

    private IEnumerator HandleMediumLand() {
        CharacterAnimation.Instance.PlayMediumLandAnim();
        yield return new WaitForSeconds(player.landIdleDuration);
        player.StateMachine.ChangeState(player.GroundedState);
    }

    private IEnumerator HandleBigLand() {
        CharacterAnimation.Instance.PlayHardLand();
        player.isFalling = false;
        yield return new WaitForSeconds(faintDuration);
        player.StateMachine.ChangeState(player.GroundedState);
    }

    public override void StateFixedUpdate() { }
}
