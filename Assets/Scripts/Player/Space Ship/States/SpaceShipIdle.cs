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
        monoBehaviour.StartCoroutine(HandleFreezeRigid());

        if(piloting) {
            monoBehaviour.StartCoroutine(HandlePlayerExitingSpaceShip());
        }
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
        trail.GetComponent<TrailManager>().targetPos = spaceShip.transform.position;
        spaceShip.coreAnimator.Play("On");

        yield return new WaitForSeconds(1.3f);

        //Change camera
        spaceShip.ProcessCameraMovement();
        CameraController.Instance.isActive = false;
        CameraManager.Instance.ChangeToBirdCamera();

        yield return new WaitForSeconds(2f);

        Instantiate(spaceShip.dustEffect, spaceShip.transform.position, spaceShip.transform.rotation);

        //Handle spaceship physics and input
        spaceShip.useGravitacionalForce = true;
        spaceShip.rigid.freezeRotation = true;
        spaceShip.rigid.isKinematic = false;
        spaceShip.inputs.Enable();
        spaceShip.audioHandler.EnterShip();

        //Change state
        spaceShip.StateMachine.ChangeState(spaceShip.OnPlanetState);
    }

    private IEnumerator HandlePlayerExitingSpaceShip() {        
        //Change spaceship to idle
        spaceShip.useGravitacionalForce = true;
        spaceShip.rigid.freezeRotation = false;
        spaceShip.inputs.Disable();
        spaceShip.audioHandler.ExitShip();

        //Change camera
        CameraController.Instance.isActive = true;
        CameraManager.Instance.ChangeToCharacterCamera();

        //effects go here
        GameObject trail = Instantiate(spaceShip.trailEffect, spaceShip.transform.position, spaceShip.transform.rotation);
        trail.GetComponent<TrailManager>().targetPos = spaceShip.playerTeleportPoint;
        spaceShip.coreAnimator.Play("Off");
        yield return new WaitForSeconds(2f);

        Instantiate(spaceShip.dissolveEffect, spaceShip.playerTeleportPoint + spaceShip.GetGravityDirection() * 0.35f, spaceShip.player.transform.rotation);
        yield return new WaitForSeconds(1f);

        //Awake player
        spaceShip.player.gameObject.SetActive(true);
        spaceShip.player.AdjustModelRotation(); 
        spaceShip.player.SetRotationToGravityDirection(); 
        spaceShip.player.transform.SetParent(null);
        spaceShip.player.transform.position = spaceShip.playerTeleportPoint;
        spaceShip.player.characterCape.ClearTransformMotion();
        spaceShip.player.inputs.Enable();
        spaceShip.player.canMove = true;

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
