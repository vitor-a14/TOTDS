using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CommunicationListener : MonoBehaviour
{
    public bool uniqueInteraction;
    public Interactable interactableScript;
    public List<Interaction> interactionSequence;
    private bool interactionRange;
    private bool interactionMatched;

    private void LateUpdate() {
        if(!interactionRange || (interactionMatched && uniqueInteraction)) return;

        interactionMatched = CommunicationHandler.Instance.MatchInteractionSequence(interactionSequence);
        if(interactionMatched) {
            interactableScript.Interact();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.tag == "Player")
            interactionRange = true;
    }

    private void OnTriggerExit(Collider other) {
        if(other.tag == "Player")
            interactionRange = false;
    }
}
