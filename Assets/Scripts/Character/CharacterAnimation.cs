using System.Collections;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    public static CharacterAnimation Instance { get; private set; }

    private Animator anim;
    private static readonly int velocity_hash = Animator.StringToHash("Velocity");
    private static readonly int stepness_hash = Animator.StringToHash("Stepness");
    private static readonly int grounded_hash = Animator.StringToHash("Grounded");
    private static readonly int near_hash = Animator.StringToHash("NearWall");

    private PlayerController player;
    private float smoothedStepness;
    private float smoothedInput;
    public float smoothedDirection;
    private float stepness;

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
        anim.SetBool(grounded_hash, player.onGround);
        anim.SetBool(near_hash, player.nearWall);
    }

    public void PlayLandAnim() {
        anim.Play("Land");
    }

    public void PlayJumpAnim() {
        anim.Play("Jump");
    }
}
