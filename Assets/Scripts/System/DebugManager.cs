using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugManager : MonoBehaviour
{
    public TMP_Text frameRateText;
    public TMP_Text posText;
    [SerializeField] private Transform bird, player;

    void Start()
    {
        StartCoroutine(UpdateFrameRateText());
    }

    IEnumerator UpdateFrameRateText() {
        while(true) {
            yield return new WaitForSeconds(0.12f);
            frameRateText.text = "FPS: " + Mathf.RoundToInt(1.0f / Time.deltaTime);

            if(BirdController.Instance.piloting) {
                posText.text = "Universal position: " + bird.position;
            } else {
                posText.text = "Universal position: " + player.position;
            }
        }
    }
}
