using System.Collections;
using UnityEngine;

public class FootSound : MonoBehaviour
{
    public CharacterFootsteps footstepsHandler;
    private bool canPlay = true;

    private void Start() {
        footstepsHandler = CharacterFootsteps.Instance;
    }

    private IEnumerator FootstepCooldownHandler() {
        canPlay = false;
        yield return new WaitForSeconds(0.05f);
        canPlay = true;
    }

    private void OnTriggerEnter(Collider other) {
        if(((1 << other.gameObject.layer) & PlayerController.Instance.walkableLayers) != 0 && canPlay) {
            footstepsHandler.PlayFootstep(1f, other.tag);
            StartCoroutine(FootstepCooldownHandler());
        }
    }
}
