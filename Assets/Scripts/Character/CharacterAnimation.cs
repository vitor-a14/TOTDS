using System.Collections;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    public static CharacterAnimation Instance { get; private set; }

    private Animator anim;
    private static readonly int velocity_hash = Animator.StringToHash("Velocity");
    private static readonly int grounded_hash = Animator.StringToHash("Grounded");
    private static readonly int jumping_hash = Animator.StringToHash("Jumping");
    private static readonly int near_hash = Animator.StringToHash("NearWall");

    [Header("Animation States")]
    public bool landing;
    public bool stoping;

    private PlayerController player;

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
        anim.SetFloat(velocity_hash, player.processedInput.magnitude);
        anim.SetBool(grounded_hash, player.onGround);
        anim.SetBool(jumping_hash, player.jumping);
        anim.SetBool(near_hash, player.nearWall);
    }

    public void PlayJumpAnim() {
        anim.Play("Jump");
    }

    public IEnumerator LandingWindow() {
        landing = true;
        yield return new WaitForSeconds(1f);
        landing = false;
    } 
}
