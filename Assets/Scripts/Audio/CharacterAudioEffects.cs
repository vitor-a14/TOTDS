using UnityEngine;

public class CharacterAudioEffects : MonoBehaviour
{
    public static CharacterAudioEffects Instance { get; private set; }

    [Header("Velocity Audio Effect")]
    public Rigidbody characterRigid; // these rigidbody will check the velocity of the player
    public Rigidbody birdRigid;
    public BirdController birdController;

    public AudioSource velocityAudioSource; // the audio source for the velocity sound effect
    public float velocityAudioThreshold; // the audio fades in when surpass this threshold
    public float lerpVelocity; // transition velocity between off/on

    [System.Serializable]
    public struct Footstep {
        public string floorTag;
        public AudioClip[] audios;
    }

    [Header("Character Footsteps")]
    public float footstepVolume;
    public Footstep[] footsteps;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
    }

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

    public void PlayFootstep(float volumeMultiplier, string floorTag) {
        foreach(Footstep footstep in footsteps) {
            if(footstep.floorTag == floorTag) {
                int randIndex = Random.Range(0, footstep.audios.Length);
                AudioManager.Instance.PlayOneShot3D(footstep.audios[randIndex], PlayerController.Instance.gameObject, AudioType.SFX, footstepVolume * volumeMultiplier);
            }
        }
    }
}
