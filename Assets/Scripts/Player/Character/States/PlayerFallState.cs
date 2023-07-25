using System.Collections;
using UnityEngine;

public class PlayerFallState : PlayerState
{
    private float fallingDuration;
    private bool handleFall;
    private MonoBehaviour monoBehaviour;

    private float faintDuration = 3f;

    public PlayerFallState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) {
        monoBehaviour = player.GetComponent<MonoBehaviour>();
    }

    public override void Enter() { 
        handleFall = true;
        fallingDuration = 0f;
        FeetIKHandler.Instance.enableFeetIK = false;
        player.isFalling = true;
    }

    public override void Exit() { 
        FeetIKHandler.Instance.enableFeetIK = true;
        player.isFalling = false;
    }

    public override void StateUpdate() {
        if(player.onGround && handleFall) {
            if(fallingDuration <= player.normalFallDuration) {
                monoBehaviour.StartCoroutine(HandleNormalFall());
            } else {
                monoBehaviour.StartCoroutine(HandleBigFall());
            }

            handleFall = false;
        } else {
            fallingDuration += Time.deltaTime;
        }
    }

    private IEnumerator HandleNormalFall() {
        CharacterAnimation.Instance.PlayLandAnim();
        yield return new WaitForSeconds(player.landIdleDuration);
        player.StateMachine.ChangeState(player.GroundedState);
    }

    private IEnumerator HandleBigFall() {
        CharacterAnimation.Instance.PlayHardLand();
        player.isFalling = false;
        yield return new WaitForSeconds(4f);
        player.StateMachine.ChangeState(player.GroundedState);
    }

    public override void StateFixedUpdate() { }
}
