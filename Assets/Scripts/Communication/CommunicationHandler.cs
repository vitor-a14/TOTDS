using System.Collections.Generic;
using UnityEngine;

//The four types of interactions/words/inputs the player has to make communication phrases
public enum Interaction {
    UP,
    DOWN,
    RIGHT,
    LEFT
}

public class CommunicationHandler : MonoBehaviour
{
    //Communication interaction variables
    public static CommunicationHandler Instance { get; private set; }
    private Queue<Interaction> interactions = new Queue<Interaction>();
    public int interactionsQueueSize; //the max number of interactions
    public float queueDuration; //the max duration the queue persist without the player interacting
    private float queueTimer  = 0; 

    //Conventional interaction variables
    private Interactable simpleInteractionTarget;
    [HideInInspector] public InteractableMessage readingTarget;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
    }

    //Clear queue after a time of non interactions were spelled
    private void LateUpdate() {
        queueTimer += Time.deltaTime;
        queueTimer = Mathf.Clamp(queueTimer, 0, queueDuration);
        if(queueTimer >= queueDuration)
            interactions.Clear();
    }

    //If the player inputs a interaction with the keys, this function is called (setup in PlayerController)
    //Add the interaction to the queue and make sure to fit the size
    public void AddInteraction(Interaction interaction) {

        if(interactions.Count >= interactionsQueueSize)
            interactions.Dequeue();

        interactions.Enqueue(interaction);
        InteractionVisualEffect(interaction);
        queueTimer = 0;
    }

    private void InteractionVisualEffect(Interaction interaction) {
        //Do visual stuff here
    }

    public bool MatchInteractionSequence(List<Interaction> listenerInteractions) {
        if(listenerInteractions.Count != interactions.Count) 
            return false;

        int i = 0;
        foreach(Interaction interaction in interactions) {
            if(interaction != listenerInteractions[i]) 
                return false;
            i++;
        }

        interactions.Clear(); //if the code get this far this means the result will be true, them we can clear the queue
        return true;
    }

    //The functions bellow handle a more conventional interaction system: push the button and interact with the object

    public void InteractWithCurrentTarget() {
        if(simpleInteractionTarget == null) return;
        simpleInteractionTarget.Interact();
    }

    private void OnTriggerEnter(Collider other) {
        if(other.GetComponent<Interactable>() != null)
            simpleInteractionTarget = other.GetComponent<Interactable>();
    }

    private void OnTriggerExit(Collider other) {
        if(other.GetComponent<Interactable>() != null)
            simpleInteractionTarget = null;
    }
}
