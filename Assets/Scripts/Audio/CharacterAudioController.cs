using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAudioController : MonoBehaviour
{
    [System.Serializable]
    public struct Footstep {
        public string floorTag;
        public AudioClip[] audios;
    }

    public float footstepVolume;
    public AudioSource audioSource;
    public Footstep[] footsteps;

    public void PlayFootstep() {
        string floorTag = player.Instance.floorTag;

        foreach(Footstep footstep in footsteps) {
            if(footstep.floorTag == floorTag) {
                int randIndex = Random.Range(0, footstep.audios.Length);
                AudioManager.Instance.PlayOneShot3D(footstep.audios[randIndex], gameObject, AudioType.SFX, footstepVolume);
            }
        }
    }
}
