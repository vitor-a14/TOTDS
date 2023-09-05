using System.Collections;
using UnityEngine;

public class SpaceshipIdle : SpaceshipState
{
    private MonoBehaviour monoBehaviour;
    private bool piloting = false;

    public SpaceshipIdle(SpaceshipController spaceship, SpaceshipStateMachine stateMachine) : base(spaceship, stateMachine) { 
        monoBehaviour = spaceship.GetComponent<MonoBehaviour>();
    }

    public override void Enter() {   
        if(piloting) {
            monoBehaviour.StartCoroutine(HandlePlayerExitingSpaceship());
        }

        monoBehaviour.StartCoroutine(HandleFreezeRigid());
    }

    public override void Interact() {
        if(!piloting) {
            monoBehaviour.StartCoroutine(HandlePlayerEnteringSpaceship());
            piloting = true;
        }
    }

    private IEnumerator HandlePlayerEnteringSpaceship() {
        //Stop player
        spaceship.player.canMove = false;

        //Effects
        Instantiate(spaceship.dissolveEffect, spaceship.player.transform.position + spaceship.player.transform.up * 0.8f, spaceship.player.transform.rotation);

        //Wait for the dissolve effect cover the player model
        yield return new WaitForSeconds(1f);

        //Hide player
        spaceship.player.DisablePlayer();
        spaceship.player.transform.SetParent(spaceship.transform);

        //Wait for the dissolve effect starting vanish to instantiate the trail
        yield return new WaitForSeconds(0.4f);

        //Trail Effect
        GameObject trail = Instantiate(spaceship.trailEffect, spaceship.player.transform.position, spaceship.player.transform.rotation);
        trail.GetComponent<TrailManager>().targetPos = spaceship.transform.position;
        spaceship.coreAnimator.Play("On");

        //Wait for the trail to move some distance to change the camera
        yield return new WaitForSeconds(1.3f);

        //Change camera
        CameraController.Instance.isActive = false;
        CameraManager.Instance.ChangeToSpaceshipCamera();

        //wait for the trail to get to the spaceship
        float waitingTime = 0f; // prevent the animation to be stuck waiting for long periods
        while(true) {
            spaceship.ProcessCameraMovement(false);
            if(trail == null || waitingTime > 8f) break;
            waitingTime += Time.deltaTime;
            yield return null;
        }

        Instantiate(spaceship.dustEffect, spaceship.transform.position, spaceship.transform.rotation);

        //Handle spaceship physics and input
        spaceship.useGravitacionalForce = false;
        spaceship.rigid.isKinematic = false;
        spaceship.rigid.drag = 3f;
        spaceship.rigid.angularDrag = 3f;
        spaceship.inputs.Enable();
        spaceship.audioHandler.EnterShip();

        //Change state
        spaceship.StateMachine.ChangeState(spaceship.OnPlanetState);
    }

    private IEnumerator HandlePlayerExitingSpaceship() {        
        //Change spaceship to idle
        spaceship.useGravitacionalForce = true;
        spaceship.rigid.drag = 1f;
        spaceship.rigid.angularDrag = 1f;
        spaceship.inputs.Disable();
        spaceship.audioHandler.ExitShip();

        TrailManager trail = Instantiate(spaceship.trailEffect, spaceship.transform.position, spaceship.transform.rotation).GetComponent<TrailManager>();
        trail.targetPos = spaceship.playerTeleportPoint;
        spaceship.coreAnimator.Play("Off");
        yield return new WaitForSeconds(1.3f);

        //Position player
        spaceship.player.transform.position = spaceship.playerTeleportPoint;

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

        Instantiate(spaceship.dissolveEffect, spaceship.playerTeleportPoint + spaceship.player.transform.up * 0.8f, spaceship.player.transform.rotation);
        yield return new WaitForSeconds(1f);

        spaceship.player.EnablePlayer();
        spaceship.player.transform.position = spaceship.playerTeleportPoint;
        piloting = false;
        yield return null;
    }

    private IEnumerator HandleFreezeRigid() {
        yield return new WaitForSeconds(0.3f);  // wait for a small time before starting check

        while(true) {
            if(spaceship.rigid.velocity.sqrMagnitude <= 0.02f) break; //if the spaceship is not moving, break the loop
            yield return new WaitForSeconds(0.1f);
        }

        spaceship.rigid.isKinematic = true;
    }
}
