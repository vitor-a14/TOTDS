using UnityEngine;

public class SpaceShipAudio : MonoBehaviour
{
    private SpaceshipController spaceshipController;
    public AudioSource motorAudioSource;
    public AudioClip enterShipSound;
    public AudioClip shipMotorSound;
    public AudioClip boostSound;
    public float maxVelocity;
    public float pitchMultiplier;
    public float pitchChangeVelocity;

    private void Start() {
        spaceshipController = SpaceshipController.Instance;
    }

    public void HandleEngineSound() { 
        float velocity = spaceshipController.rigid.velocity.sqrMagnitude / maxVelocity;
        float newPitch = Mathf.Clamp(pitchMultiplier * velocity, 1f, 2f);
        motorAudioSource.pitch = Mathf.Lerp(motorAudioSource.pitch, newPitch, pitchChangeVelocity * Time.deltaTime);
    }

    public void EnterShip() {
        AudioManager.Instance.PlayOneShot2D(enterShipSound, gameObject, 1); //debug
        motorAudioSource.Play();
    }

    public void ExitShip() {
        motorAudioSource.pitch = 0f;
        motorAudioSource.Pause();
    }

    public void Boost() {
        AudioManager.Instance.PlayOneShot2D(boostSound, gameObject, 0.05f); //debug
    }
}
