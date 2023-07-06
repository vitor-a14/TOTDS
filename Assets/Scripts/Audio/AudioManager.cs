using UnityEngine;

public enum AudioType {
    SFX,
    Ambience,
    Music
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Settings")]
    [Range(0, 10)] public float masterVolume;
    [Range(0, 1)] public float SFXVolume;
    [Range(0, 1)] public float ambienceVolume;
    [Range(0, 1)] public float musicVolume;

    private void Awake() {
        if(Instance == null)
            Instance = this;
        else
            Debug.LogError(this.name + " is trying to set a Instance, but seems like a instance is already attributed.");

        AudioListener.volume = 1;
    }

    //Create a 3D audio instance in a gameobject
    public void PlayOneShot3D(AudioClip audio, GameObject entity, AudioType type, float multiplier) {
        AudioSource audioSource = entity.AddComponent<AudioSource>();

        switch(type) {
        case AudioType.SFX:
            audioSource.volume = SFXVolume;
            break;
        case AudioType.Ambience:
            audioSource.volume = ambienceVolume;
            break;
        case AudioType.Music:
            audioSource.volume = musicVolume;
            break;
        }

        audioSource.spatialBlend = 1;
        audioSource.volume *= multiplier;
        audioSource.volume *= masterVolume;
        audioSource.PlayOneShot(audio);
        
        Destroy(audioSource, audio.length);
    }

    //Create a 2D audio instance in a gameobject
    public void PlayOneShot2D(AudioClip audio, GameObject entity, AudioType type, float multiplier) {
        AudioSource audioSource = entity.AddComponent<AudioSource>();

        switch(type) {
        case AudioType.SFX:
            audioSource.volume = SFXVolume;
            break;
        case AudioType.Ambience:
            audioSource.volume = ambienceVolume;
            break;
        case AudioType.Music:
            audioSource.volume = musicVolume;
            break;
        }

        audioSource.spatialBlend = 0;
        audioSource.volume *= multiplier;
        audioSource.volume *= masterVolume;
        audioSource.PlayOneShot(audio);
        
        Destroy(audioSource, audio.length);
    }

    //Use a already existent audio source to play a audio
    public void PlayOnAudioSorce(AudioClip audio, AudioSource audioSource, AudioType type, float multiplier) {
        switch(type) {
        case AudioType.SFX:
            audioSource.volume = SFXVolume;
            break;
        case AudioType.Ambience:
            audioSource.volume = ambienceVolume;
            break;
        case AudioType.Music:
            audioSource.volume = musicVolume;
            break;
        }

        audioSource.volume *= multiplier;
        audioSource.volume *= masterVolume;
        audioSource.clip = audio;
        audioSource.Play();
    }
}
