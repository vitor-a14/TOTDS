using UnityEngine;

public class CharacterFootsteps : MonoBehaviour
{
    public static CharacterFootsteps Instance { get; private set; }

    [System.Serializable]
    public struct Footstep {
        public string floorTag;
        public AudioClip[] audios;
    }

    public float footstepVolume;
    public Footstep[] footsteps;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
    }

    public void PlayFootstep() {
        string floorTag = PlayerController.Instance.floorTag;

        foreach(Footstep footstep in footsteps) {
            if(footstep.floorTag == floorTag) {
                int randIndex = Random.Range(0, footstep.audios.Length);
                AudioManager.Instance.PlayOneShot3D(footstep.audios[randIndex], PlayerController.Instance.gameObject, AudioType.SFX, footstepVolume);
            }
        }
    }
}
