using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableMessage : Interactable
{
    public int id;
    private Message message;
    private int currentMessage = 0;
    private string[] messageText;

    public override void Interact() {
        message = MessagesLoader.Instance.FindMessage(id);
        if(message == null) return;

        foreach(TextContent content in message.content) {
            if(content.language == Settings.Instance.language)
                messageText = content.texts;
        }

        CommunicationHandler.Instance.readingTarget = this;
        //PlayerController.Instance.reading = true;
        currentMessage = -1;

        ShowMessage();
    }

    public void ShowMessage() {
        currentMessage++;
        if(currentMessage > messageText.Length - 1) {
            UIManager.Instance.HideText();
            //PlayerController.Instance.reading = false;
        } else {
            UIManager.Instance.ShowText(messageText[currentMessage]);
        }
    }
}
