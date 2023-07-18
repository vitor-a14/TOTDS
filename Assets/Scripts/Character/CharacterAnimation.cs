using System.Collections;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    public static CharacterAnimation Instance { get; private set; }

    private Animator anim;
    private static readonly int velocity_hash = Animator.StringToHash("Velocity");

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
        anim.SetFloat(velocity_hash, player.input.magnitude);
        anim.SetBool("Grounded", player.onGround);
        anim.SetBool("Jumping", player.jumping);

        //anim.CrossFade(GetCurrentState(), 0, 0);
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
