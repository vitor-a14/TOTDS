using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class FootSoundTrigger : MonoBehaviour
{
    private PlayerController player;

    public AudioSource footAudio;
    public string floorTag;

    private void Start() {
        player = PlayerController.Instance;
    }

    private void OnTriggerEnter(Collider other) {
        if(((1 << other.gameObject.layer) & player.walkableLayers) != 0) {
            floorTag = other.tag;
        }
    }
}
