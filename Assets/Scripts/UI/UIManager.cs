using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TMP_Text messageText;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
    }

    public void ShowText(string text) {
        messagePanel.SetActive(true);
        messageText.text = text;
    }

    public void HideText() {
        messageText.text = "";
        messagePanel.SetActive(false);
    }
}
