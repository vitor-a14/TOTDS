using UnityEngine;
using UnityEngine.Audio;

public enum AudioType {
    SFX,
    Ambience,
    Music
}

public enum AudioSnapshot {
    OUTDOOR,
    INDOOR,
    SPACE,
    NONE
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Settings")]
    public AudioMixerGroup audioMixer;
    public AudioMixerSnapshot indoorAudioSnapshot;
    public AudioMixerSnapshot outsideAudioSnapshot;
    public AudioMixerSnapshot spaceAudioSnapshot;

    public AudioSource outdoorAudioSource;
    public AudioSource indoorAudioSource;

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

    public void ChangeAudioSnapshot(AudioSnapshot snapshot, float transitionDuration) {
        if(snapshot == AudioSnapshot.INDOOR)
            indoorAudioSnapshot.TransitionTo(transitionDuration);
        else if(snapshot == AudioSnapshot.OUTDOOR)
            outsideAudioSnapshot.TransitionTo(transitionDuration);
        else if(snapshot == AudioSnapshot.SPACE)
            spaceAudioSnapshot.TransitionTo(transitionDuration);
    }

    //Create a 3D audio instance in a gameobject
    public void PlayOneShot3D(AudioClip audio, GameObject entity, AudioType type, float multiplier) {
        AudioSource audioSource = entity.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = audioMixer;

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
        audioSource.outputAudioMixerGroup = audioMixer;

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
        if(audioSource.outputAudioMixerGroup == null)
            audioSource.outputAudioMixerGroup = audioMixer;

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