using System.Collections;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    public static CharacterAnimation Instance { get; private set; }

    private Animator anim;
    private static readonly int idle_hash = Animator.StringToHash("Idle");
    private static readonly int walk_hash = Animator.StringToHash("Walk");
    private static readonly int run_hash = Animator.StringToHash("Run");
    private static readonly int fall_hash = Animator.StringToHash("Fall");
    private static readonly int jump_hash = Animator.StringToHash("Jump");
    private static readonly int land_hash = Animator.StringToHash("Land");
    private static readonly int stop_hash = Animator.StringToHash("Stop");

    [Header("Animation States")]
    public bool landing;
    public bool stoping;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
    }

    void Start() {
        anim = GetComponent<Animator>();
    }

    void Update() {
        anim.CrossFade(GetCurrentState(), 0, 0);
    }

    private int GetCurrentState() {
        if(!PlayerController.Instance.onGround) {
            if(PlayerController.Instance.jumping) 
                return jump_hash;
            else
                return fall_hash;
        } else {
            if(landing) 
                return land_hash;
            else {
                if(PlayerController.Instance.direction.magnitude > PlayerController.Instance.movementSpeed / 2) 
                    return run_hash;
                else if(PlayerController.Instance.direction.magnitude < PlayerController.Instance.movementSpeed / 2 && PlayerController.Instance.direction.magnitude != 0) 
                    return walk_hash;
                else 
                    return idle_hash;
            }
        }
    } 

    public IEnumerator LandingWindow() {
        landing = true;
        yield return new WaitForSeconds(1f);
        landing = false;
    } 
}
