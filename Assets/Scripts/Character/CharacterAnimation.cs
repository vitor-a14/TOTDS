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

    public Transform feetIK;

    [Header("Animation States")]
    public bool landing;
    public bool stoping;

    private player player;

    void OnAnimatorIK() {
        if(anim) {
            anim.SetLookAtWeight(1);
            anim.SetLookAtPosition(Camera.main.transform.position);

            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);  
            anim.SetIKPosition(AvatarIKGoal.LeftFoot, feetIK.position);
            anim.SetIKRotation(AvatarIKGoal.LeftFoot, feetIK.rotation);
        }
    }

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
    }

    void Start() {
        anim = GetComponent<Animator>();
        player = player.Instance;
    }

    void Update() {
        anim.CrossFade(GetCurrentState(), 0, 0);
    }

    private int GetCurrentState() {
        if(!player.onGround) {
            if(player.jumping) 
                return jump_hash;
            else
                return fall_hash;
        } else {
            if(landing) 
                return land_hash;
            else {
                if(player.direction.magnitude > player.movementSpeed / 1.9f) 
                    return run_hash;
                else if(player.Instance.direction.magnitude < player.movementSpeed / 1.9f && player.direction.magnitude != 0) 
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
