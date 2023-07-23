using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FootSoundTrigger : MonoBehaviour
{
    private PlayerController playerController;

    public AudioSource footAudio;
    public string floorTag;

    private void Start() {
        playerController = PlayerController.Instance;
    }

    private void OnTriggerEnter(Collider other) {
        if(((1 << other.gameObject.layer) & playerController.walkableLayers) != 0) {
            floorTag = other.tag;
        }
    }
}
