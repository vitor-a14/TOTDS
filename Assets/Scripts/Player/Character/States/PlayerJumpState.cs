using System.Collections;
using UnityEngine;

public class PlayerJumpState : PlayerState
{
    private Vector3 direction;
    private MonoBehaviour monoBehaviour;

    private float idleJumpDuration = 1f;
    private float runningJumpDuration = 0.18f;
    private float landingJumpDuration = 0.22f;

    public PlayerJumpState(PlayerController player, PlayerStateMachine stateMachine) : base(player, stateMachine) {
        monoBehaviour = player.GetComponent<MonoBehaviour>();
    }

    public override void Enter() {
        if(player.input.sqrMagnitude <= 0.1f) {
            monoBehaviour.StartCoroutine(HandleIdleJumpCoroutine());
        } else {
            monoBehaviour.StartCoroutine(HandleRunningJumpCoroutine());
        }
    }

    public override void StateUpdate() { }

    public override void StateFixedUpdate() { }

    private IEnumerator HandleIdleJumpCoroutine() {
        CharacterAnimation.Instance.PlayIdleJumpAnim();
        yield return new WaitForSeconds(idleJumpDuration);
        player.StateMachine.ChangeState(player.GroundedState);
    }  

    private IEnumerator HandleRunningJumpCoroutine() {
        CharacterAnimation.Instance.PlayRunningJumpAnim();
        yield return new WaitForSeconds(runningJumpDuration);
        player.rigid.AddForce(player.characterModel.up * player.jumpForce, ForceMode.VelocityChange);
        player.rigid.AddForce(player.characterModel.forward * player.jumpForce, ForceMode.VelocityChange);
        yield return new WaitForSeconds(0.05f);
        player.StateMachine.ChangeState(player.FallState);
    }

    public override void Exit() { }
}
