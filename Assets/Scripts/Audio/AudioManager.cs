using UnityEngine;
using UnityEngine.Audio;

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

    //If you searching where to control the sounds, use the Audio Mixer in the audio folders

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
    public void PlayOneShot3D(AudioClip audio, GameObject entity, float volume) {
        AudioSource audioSource = entity.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = audioMixer;

        audioSource.spatialBlend = volume;
        audioSource.volume = 1f;
        audioSource.PlayOneShot(audio);
        
        Destroy(audioSource, audio.length);
    }

    //Create a 2D audio instance in a gameobject
    public void PlayOneShot2D(AudioClip audio, GameObject entity, float volume) {
        AudioSource audioSource = entity.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = audioMixer;

        audioSource.spatialBlend = 0;
        audioSource.volume = volume;
        audioSource.PlayOneShot(audio);
        
        Destroy(audioSource, audio.length);
    }

    //Use a already existent audio source to play a audio
    public void PlayOnAudioSorce(AudioClip audio, AudioSource audioSource, float volume) {
        if(audioSource.outputAudioMixerGroup == null)
            audioSource.outputAudioMixerGroup = audioMixer;

        audioSource.volume = volume;
        audioSource.clip = audio;
        audioSource.Play();
    }
}
