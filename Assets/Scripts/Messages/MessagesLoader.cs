using System;
using System.IO;
using UnityEngine;

public class MessagesLoader : MonoBehaviour
{
    public static MessagesLoader Instance { get; private set; }
    [SerializeField] private TextAsset messagesJson;

    public Messages messageData;

    public void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");

        messageData = LoadJson();
    }

    private Messages LoadJson()
    {
        Messages loadedMessagesData = null;

        try {
            loadedMessagesData = JsonUtility.FromJson<Messages>(messagesJson.ToString());
        } catch(Exception e) {
            Debug.LogError("Error while trying to deserialize json " + e);
        }  

        return loadedMessagesData;
    }

    public Message FindMessage(int id) {
        foreach(Message message in messageData.messages) {
            if(message.id == id)
                return message;
        }

        Debug.LogWarning("No messages with id " + id + " found.");
        return null;
    }
}
