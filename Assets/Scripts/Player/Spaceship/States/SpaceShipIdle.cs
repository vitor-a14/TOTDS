using System.Collections;
using UnityEngine;

public class SpaceShipIdle : SpaceShipState
{
    private MonoBehaviour monoBehaviour;
    private bool piloting = false;

    public SpaceShipIdle(SpaceShipController spaceShip, SpaceShipStateMachine stateMachine) : base(spaceShip, stateMachine) { 
        monoBehaviour = spaceShip.GetComponent<MonoBehaviour>();
    }

    public override void Enter() {   
        if(piloting) {
            monoBehaviour.StartCoroutine(HandlePlayerExitingSpaceShip());
        }

        monoBehaviour.StartCoroutine(HandleFreezeRigid());
    }

    public override void Interact() {
        if(!piloting) {
            monoBehaviour.StartCoroutine(HandlePlayerEnteringSpaceShip());
            piloting = true;
        }
    }

    private IEnumerator HandlePlayerEnteringSpaceShip() {
        //Stop player
        spaceShip.player.canMove = false;

        //Effects
        Instantiate(spaceShip.dissolveEffect, spaceShip.player.transform.position + spaceShip.player.transform.up * 0.8f, spaceShip.player.transform.rotation);

        //Wait for the dissolve effect cover the player model
        yield return new WaitForSeconds(1f);

        //Hide player
        spaceShip.player.DisablePlayer();
        spaceShip.player.transform.SetParent(spaceShip.transform);

        //Wait for the dissolve effect starting vanish to instantiate the trail
        yield return new WaitForSeconds(0.4f);

        //Trail Effect
        GameObject trail = Instantiate(spaceShip.trailEffect, spaceShip.player.transform.position, spaceShip.player.transform.rotation);
        trail.GetComponent<TrailManager>().targetPos = spaceShip.transform.position;
        spaceShip.coreAnimator.Play("On");

        //Wait for the trail to move some distance to change the camera
        yield return new WaitForSeconds(1.3f);

        //Change camera
        spaceShip.ProcessCameraMovement();
        CameraController.Instance.isActive = false;
        CameraManager.Instance.ChangeToBirdCamera();

        //wait for the trail to get to the spaceship
        float waitingTime = 0f; // prevent the animation to be stuck waiting for long periods
        while(true) {
            if(trail == null || waitingTime > 8f) break;
            waitingTime += Time.deltaTime;
            yield return null;
        }

        Instantiate(spaceShip.dustEffect, spaceShip.transform.position, spaceShip.transform.rotation);

        //Handle spaceship physics and input
        spaceShip.useGravitacionalForce = false;
        spaceShip.rigid.isKinematic = false;
        spaceShip.rigid.drag = 3f;
        spaceShip.rigid.angularDrag = 3f;
        spaceShip.inputs.Enable();
        spaceShip.audioHandler.EnterShip();

        //Change state
        spaceShip.StateMachine.ChangeState(spaceShip.OnPlanetState);
    }

    private IEnumerator HandlePlayerExitingSpaceShip() {        
        //Change spaceship to idle
        spaceShip.useGravitacionalForce = true;
        spaceShip.rigid.drag = 1f;
        spaceShip.rigid.angularDrag = 1f;
        spaceShip.inputs.Disable();
        spaceShip.audioHandler.ExitShip();

        TrailManager trail = Instantiate(spaceShip.trailEffect, spaceShip.transform.position, spaceShip.transform.rotation).GetComponent<TrailManager>();
        trail.targetPos = spaceShip.playerTeleportPoint;
        spaceShip.coreAnimator.Play("Off");
        yield return new WaitForSeconds(1.3f);

        //Position player
        spaceShip.player.transform.position = spaceShip.playerTeleportPoint;

        //Change camera
        CameraController.Instance.isActive = true;
        CameraManager.Instance.ChangeToCharacterCamera();

        //wait for the trail to get to the character
        float waitingTime = 0f; // prevent the animation to be stuck waiting for long periods
        while(true) {
            if(trail.destinityReached || waitingTime > 8f) break;
            waitingTime += Time.deltaTime;
            yield return null;
        }

        Instantiate(spaceShip.dissolveEffect, spaceShip.playerTeleportPoint + spaceShip.player.transform.up * 0.8f, spaceShip.player.transform.rotation);
        yield return new WaitForSeconds(1f);

        spaceShip.player.EnablePlayer();
        spaceShip.player.transform.position = spaceShip.playerTeleportPoint;
        piloting = false;
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
