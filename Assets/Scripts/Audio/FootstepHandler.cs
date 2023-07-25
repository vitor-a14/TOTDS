using UnityEngine;

public class FootstepHandler : MonoBehaviour
{
    [System.Serializable]
    public struct Footstep {
        public string floorTag;
        public AudioClip[] audios;
    }

    [Header("Character Footsteps")]
    public float footstepVolume;
    public Footstep[] footsteps;
    public Transform leftFoot, rightFoot;
    public AudioSource leftFootAudio, rightFootAudio;

    private bool useRightFoot = false;
    private int lastIndex;
    private int randIndex;

    private RaycastHit hit;
    private Transform foot;
    private AudioSource audioSource;
    private string floorTag = "";

    private PlayerController player;

    private void Start() {
        player = PlayerController.Instance;
    }

    public void PlayFootstep(AnimationEvent animationEvent) {
        if(animationEvent.animatorClipInfo.weight < 0.5f) return;

        if(useRightFoot) {
            foot = rightFoot;
            audioSource = rightFootAudio;
        } else { 
            foot = leftFoot;
            audioSource = leftFootAudio;
        }

        useRightFoot = !useRightFoot;

        if(Physics.Raycast(foot.position + transform.up * 0.1f, -transform.up, out hit, 1.2f, player.walkableLayers)) {
            floorTag = hit.transform.tag;
        }

        foreach(Footstep footstep in footsteps) {
            if (footstep.floorTag == floorTag) {
                randIndex = Random.Range(0, footstep.audios.Length);

                if(randIndex == lastIndex) {
                    if(randIndex == footstep.audios.Length - 1)
                        randIndex = 0;
                    else if(randIndex == 0)
                        randIndex = footstep.audios.Length - 1;
                    else 
                        randIndex ++;
                }

                lastIndex = randIndex;

                if(audioSource.isPlaying)
                    audioSource.Stop();

                if(audioSource.volume != footstepVolume)
                    audioSource.volume = footstepVolume;

                audioSource.PlayOneShot(footstep.audios[randIndex]);
                
                break;
            }
        }
    }
}
