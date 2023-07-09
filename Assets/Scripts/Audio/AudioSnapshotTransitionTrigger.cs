using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AudioSnapshotTransitionTrigger : MonoBehaviour
{
    public AudioSnapshot onEnterSnapshot, onExitSnapshot;
    private AudioManager audioManager;

    private void Start() {
        audioManager = AudioManager.Instance;
    }

    private void OnTriggerEnter(Collider other) {
        if(other.transform.tag == "Player" && onEnterSnapshot != AudioSnapshot.NONE) {
            audioManager.ChangeAudioSnapshot(onEnterSnapshot);
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.transform.tag == "Player" && onEnterSnapshot != AudioSnapshot.NONE) {
            audioManager.ChangeAudioSnapshot(onExitSnapshot);
        }
    }
}
