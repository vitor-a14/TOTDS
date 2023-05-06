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
    public static CommunicationHandler Instance { get; private set; }
    private Queue<Interaction> interactions = new Queue<Interaction>();
    public int interactionsQueueSize; //the max number of interactions
    public float queueDuration; //the max duration the queue persist without the player interacting
    private float queueTimer  = 0; 

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
    }

    private void LateUpdate() {
        queueTimer += Time.deltaTime;
        queueTimer = Mathf.Clamp(queueTimer, 0, queueDuration);
    }

    //If the player inputs a interaction with the keys, this function is called (setup in PlayerController)
    //Add the interaction to the queue and make sure to fit the size
    public void AddInteraction(Interaction interaction) {
        if(queueTimer >= queueDuration)
            interactions.Clear();

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

        return true;
    }
}
