using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdController : Interactable
{
    public bool piloting = false;
    public float changeModeDuration;

    [SerializeField] private Transform player;
    private Inputs inputs;

    private void Awake() {
        inputs = new Inputs();
        inputs.Enable();

        inputs.Bird.Exit.performed += ctx => ExitPilotMode();
    }

    public override void Interact() {
        EnterPilotMode();
    }

    //This will hide the player and enter the "Bird Mode" to control the vehicle
    public void EnterPilotMode() {
        if(!PlayerController.Instance.canMove && PlayerController.Instance.reading) return;
        StartCoroutine(EnterPilotModeCoroutine());
    }

    private IEnumerator EnterPilotModeCoroutine() {
        yield return new WaitForSeconds(changeModeDuration); //todo
        piloting = true;
        player.SetParent(transform);
        player.gameObject.SetActive(false);
    }

    public void ExitPilotMode() {
        if(!piloting) return;
        StartCoroutine(ExitPilotModeCoroutine());
    }

    private IEnumerator ExitPilotModeCoroutine() {
        yield return new WaitForSeconds(changeModeDuration); //todo
        piloting = false;
        player.gameObject.SetActive(true);
        player.SetParent(null);
    }
}
