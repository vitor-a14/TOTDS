using System.Collections;
using System.Linq;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    public static CharacterAnimation Instance { get; private set; }

    //Dependencies
    private Animator anim;
    private PlayerController player;
    private float smoothedStepness;
    private float smoothedInput;
    private float stepness;

    //Animation variables
    private static readonly int velocity_hash = Animator.StringToHash("Velocity");
    private static readonly int stepness_hash = Animator.StringToHash("Stepness");
    private static readonly int falling_hash = Animator.StringToHash("Falling");
    private static readonly int grounded_hash = Animator.StringToHash("OnGround");
    private static readonly int edge_hash = Animator.StringToHash("OnEdge");

    //Animation states
    private static readonly int jump_idle_hash = Animator.StringToHash("Jump Idle");
    private static readonly int jump_moving_hash = Animator.StringToHash("Jump Moving");
    private static readonly int fall_hash = Animator.StringToHash("Fall");
    private static readonly int small_land_idle_hash = Animator.StringToHash("Small Land Idle");
    private static readonly int small_land_moving_hash = Animator.StringToHash("Small Land Moving");
    private static readonly int medium_land_hash = Animator.StringToHash("Medium Land");
    private static readonly int hard_land_hash = Animator.StringToHash("Hard Land");

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
    }

    void Start() {
        anim = GetComponent<Animator>();
        player = PlayerController.Instance;
    }

    void Update() {
        if(player.canMove) {
            if(player.stopCharacterNearWalls) {
                if(!player.nearWall)
                    smoothedInput = player.processedInput.sqrMagnitude;
                else 
                    smoothedInput = Mathf.Lerp(smoothedInput, 0, 5 * Time.fixedDeltaTime);
            } else {
                smoothedInput = player.processedInput.sqrMagnitude;
            }
        } else {
            smoothedInput = Mathf.Lerp(smoothedInput, 0, 5 * Time.fixedDeltaTime);
        }

        stepness = player.onSlope == true ? 1 : 0;
        smoothedStepness = Mathf.Lerp(smoothedStepness, stepness, 5 * Time.deltaTime);

        anim.SetFloat(velocity_hash, smoothedInput);
        anim.SetFloat(stepness_hash, smoothedStepness);
        anim.SetBool(falling_hash, player.isFalling);
        anim.SetBool(grounded_hash, player.onGround);
        anim.SetBool(edge_hash, player.onEdge);
    }

    public float GetCurrentAnimationLength() {
        return anim.GetCurrentAnimatorClipInfo(0).Length;
    }

    public void PlayIdleJumpAnim() {
        anim.Play(jump_idle_hash);
    }

    public void PlayRunningJumpAnim() {
        if(anim.IsInTransition(0)) return;
        anim.CrossFadeInFixedTime(jump_moving_hash, 0.2f);
    }

    public void PlayFallAnim() {
        if(anim.IsInTransition(0)) return;
        anim.CrossFadeInFixedTime(fall_hash, 0.2f);
    }

    public void PlaySmallLandMoving() {
        if(anim.IsInTransition(0)) return;
        anim.CrossFadeInFixedTime(small_land_moving_hash, 0.12f);
    }

    public void PlaySmallLandIdle() {
        if(anim.IsInTransition(0)) return;
        anim.CrossFadeInFixedTime(small_land_idle_hash, 0.12f);
    }

    public void PlayMediumLandAnim() {
        anim.Play(medium_land_hash);
    }

    public void PlayHardLand() {
        anim.Play(hard_land_hash);
    }
}
