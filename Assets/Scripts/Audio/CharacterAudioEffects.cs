using UnityEngine;

public class CharacterAudioEffects : MonoBehaviour
{
    [Header("Velocity Audio Effect")]
    public Rigidbody characterRigid; // these rigidbody will check the velocity of the player
    public Rigidbody birdRigid;
    public BirdController birdController;

    public AudioSource velocityAudioSource; // the audio source for the velocity sound effect
    public float velocityAudioThreshold; // the audio fades in when surpass this threshold
    public float lerpVelocity; // transition velocity between off/on

    void LateUpdate() {
        VelocityAudioEffect();
    }

    private void VelocityAudioEffect() {
        Rigidbody rigid;
        
        if(birdController.piloting) {
            rigid = birdRigid;
        } else {
            rigid = characterRigid;
        }

        if(rigid.velocity.magnitude >= velocityAudioThreshold) {
            velocityAudioSource.volume = Mathf.Lerp(velocityAudioSource.volume, 1, rigid.velocity.magnitude / 30f * lerpVelocity * Time.deltaTime);
        } else {
            velocityAudioSource.volume = Mathf.Lerp(velocityAudioSource.volume, 0, 5 * lerpVelocity * Time.deltaTime);
        }
    }
}
