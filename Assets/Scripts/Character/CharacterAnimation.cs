using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private Animator anim;
    private static readonly int idle = Animator.StringToHash("Idle");
    private static readonly int walk = Animator.StringToHash("Walk");
    private static readonly int run = Animator.StringToHash("Run");
    private static readonly int fall = Animator.StringToHash("Fall");

    void Start() {
        anim = GetComponent<Animator>();
    }

    void Update() {
        anim.CrossFade(GetCurrentState(), 0, 0);
    }

    private int GetCurrentState() {
        if(!CharacterController.Instance.onGround)
            return fall;
        else {
            if(CharacterController.Instance.direction.magnitude > CharacterController.Instance.movementSpeed / 2) 
                return run;
            else if(CharacterController.Instance.direction.magnitude < CharacterController.Instance.movementSpeed / 2 && CharacterController.Instance.direction.magnitude != 0) 
                return walk;
            else 
                return idle;
        }
    }
}
