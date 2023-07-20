using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FootSoundTrigger : MonoBehaviour
{
    private CharacterAudioEffects characterAudio;
    private PlayerController playerController;
    private bool canPlay = true;

    private void Start() {
        characterAudio = CharacterAudioEffects.Instance;
        playerController = PlayerController.Instance;
    }

    private IEnumerator FootstepCooldownHandler() {
        canPlay = false;
        yield return new WaitForSeconds(0.05f);
        canPlay = true;
    }

    private void OnTriggerEnter(Collider other) {
        if(((1 << other.gameObject.layer) & playerController.walkableLayers) != 0 && canPlay) {
            characterAudio.PlayFootstep(1f, other.tag);
            StartCoroutine(FootstepCooldownHandler());
        }
    }
}
