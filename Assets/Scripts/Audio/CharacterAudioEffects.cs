using UnityEngine;

public class CharacterAudioEffects : MonoBehaviour
{
    public static CharacterAudioEffects Instance { get; private set; }

    [Header("Velocity Audio Effect")]
    public Rigidbody characterRigid; 
    public Rigidbody spaceShipRigid;

    public AudioSource velocityAudioSource; // the audio source for the velocity sound effect
    public float velocityAudioThreshold; // the audio fades in when surpass this threshold
    public float lerpVelocity; // transition velocity between off/on

    private GameObject player;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
    }

    private void Start() {
        player = PlayerController.Instance.gameObject;
    }

    void LateUpdate() {
        VelocityAudioEffect();
    }

    private void VelocityAudioEffect() {
        Rigidbody rigid;

        //todo: improve this section
        if(player.activeSelf) {
            rigid = characterRigid;
        } else {
            rigid = spaceShipRigid;
        }

        if(rigid.velocity.magnitude >= velocityAudioThreshold) {
            velocityAudioSource.volume = Mathf.Lerp(velocityAudioSource.volume, 1, rigid.velocity.magnitude / 30f * lerpVelocity * Time.deltaTime);
        } else {
            velocityAudioSource.volume = Mathf.Lerp(velocityAudioSource.volume, 0, 5 * lerpVelocity * Time.deltaTime);
        }
    }
}
