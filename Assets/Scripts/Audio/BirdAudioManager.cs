using UnityEngine;

public class BirdAudioManager : MonoBehaviour
{
    private BirdController birdController;
    public AudioSource motorAudioSource;
    public AudioClip enterShipSound;
    public AudioClip shipMotorSound;
    public AudioClip boostSound;
    public float maxVelocity;
    public float pitchMultiplier;

    private void Start() {
        birdController = BirdController.Instance;
        ExitShip();
    }

    private void Update() {
        if(birdController.piloting) {
            float velocity = birdController.physics.rigid.velocity.magnitude / maxVelocity;
            velocity = Mathf.Clamp(velocity, 0, 1);
            motorAudioSource.pitch = 1 + (pitchMultiplier * velocity);
        }
    }

    public void EnterShip() {
        motorAudioSource.spatialBlend = 0;
        motorAudioSource.Play();
        AudioManager.Instance.PlayOneShot2D(enterShipSound, gameObject, AudioType.SFX, 1);
    }

    public void ExitShip() {
        motorAudioSource.spatialBlend = 1;
        motorAudioSource.pitch = 1;
        motorAudioSource.Play();
    }

    public void Boost() {
        AudioManager.Instance.PlayOneShot2D(boostSound, gameObject, AudioType.SFX, 0.05f);
    }
}
