using System.Collections;
using UnityEngine;

public class SpaceShipIdle : SpaceShipState
{
    private MonoBehaviour monoBehaviour;

    public SpaceShipIdle(SpaceShipController spaceShip, SpaceShipStateMachine stateMachine) : base(spaceShip, stateMachine) { 
        monoBehaviour = spaceShip.GetComponent<MonoBehaviour>();
    }

    public override void Enter() { 
        monoBehaviour.StartCoroutine(HandlePlayerExitingSpaceShip());
        monoBehaviour.StartCoroutine(HandleFreezeRigid());
    }

    public override void Interact() {
        monoBehaviour.StartCoroutine(HandlePlayerEnteringSpaceShip());
    }

    private IEnumerator HandlePlayerEnteringSpaceShip() {
        //Stop player
        spaceShip.player.canMove = false;
        spaceShip.player.inputs.Disable();

        CameraManager.Instance.PanOutCameraEffect(2f);

        //Effects
        Instantiate(spaceShip.dissolveEffect, spaceShip.player.transform.position, spaceShip.player.transform.rotation);
        yield return new WaitForSeconds(1f);

        //Hide player
        spaceShip.player.transform.SetParent(spaceShip.transform);
        spaceShip.player.gameObject.SetActive(false);

        yield return new WaitForSeconds(0.4f);

        //Trail Effect
        GameObject trail = Instantiate(spaceShip.trailEffect, spaceShip.player.transform.position, spaceShip.player.transform.rotation);
        trail.GetComponent<TrailManager>().target = spaceShip.transform;
        spaceShip.coreAnimator.Play("On");

        yield return new WaitForSeconds(1.3f);

        //Change camera
        spaceShip.ProcessCameraMovement();
        CameraController.Instance.isActive = false;
        CameraManager.Instance.ChangeToBirdCamera();

        yield return new WaitForSeconds(1f);

        //Handle spaceship physics and input
        spaceShip.useGravitacionalForce = false;
        spaceShip.rigid.freezeRotation = true;
        spaceShip.rigid.isKinematic = false;
        spaceShip.inputs.Enable();
        spaceShip.audioHandler.EnterShip();

        //Change state
        spaceShip.StateMachine.ChangeState(spaceShip.OnPlanetState);
    }

    private IEnumerator HandlePlayerExitingSpaceShip() {
        spaceShip.useGravitacionalForce = true;
        spaceShip.rigid.freezeRotation = false;
        yield return null;
    }

    private IEnumerator HandleFreezeRigid() {
        yield return new WaitForSeconds(0.3f);  // wait for a small time before starting check

        while(true) {
            if(spaceShip.rigid.velocity.sqrMagnitude <= 0.02f) break; //if the spaceship is not moving, break the loop
            yield return new WaitForSeconds(0.1f);
        }

        spaceShip.rigid.isKinematic = true;
    }
}
