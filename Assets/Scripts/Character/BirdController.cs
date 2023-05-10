using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdController : Interactable
{
    [Header("Movement Settings")]
    public bool piloting = false;
    public float changeModeDuration;
    public float movementSpeed;
    public float torqueSpeed;
    public float boostForce;
    public float boostSpeed;
    public float boostCooldown;
    public float exitBoostModeTrigger;
    private float boostTimer;
    public bool speedMode = false;
    public bool canCheckBoostModeStatus = true;

    [Header("Camera Settings")]
    public Vector3 cameraOffset;
    public float cameraFollowSpeed, cameraRotationSpeed;
    [SerializeField] private Transform cam;

    [Header("Dependencies")]
    [SerializeField] private Transform player;
    [SerializeField] private PhysicsObject physics;
    private Inputs inputs;
    private Vector2 inputMove, inputRotate;
    private float inputAltitude;
    private float inputRoll;
    private Vector3 direction, torqueRotation;

    private void Awake() {
        inputs = new Inputs();
        inputs.Enable();

        inputs.Bird.Exit.performed += ctx => ExitPilotMode();
        inputs.Bird.Move.performed += ctx => inputMove = ctx.ReadValue<Vector2>();
        inputs.Bird.Move.canceled += ctx => inputMove = Vector2.zero;
        inputs.Bird.Altitude.performed += ctx => inputAltitude = ctx.ReadValue<float>();
        inputs.Bird.Altitude.canceled += ctx => inputAltitude = 0;
        inputs.Bird.Rotate.performed += ctx => inputRotate = ctx.ReadValue<Vector2>();
        inputs.Bird.Rotate.canceled += ctx => inputRotate = Vector2.zero;
        inputs.Bird.Roll.performed += ctx => inputRoll = ctx.ReadValue<float>();
        inputs.Bird.Roll.canceled += ctx => inputRoll = 0;
        inputs.Bird.Boost.performed += ctx => Boost();
    }

    private void Update() {
        boostTimer += Time.deltaTime;
        boostTimer = Mathf.Clamp(boostTimer, 0, boostCooldown);
        if(!piloting) return;
        BirdInput();
    }

    private void FixedUpdate() {
        if(!piloting) return;
        ProcessBirdMovement();
        ProcessCameraMovement();
    }

    private void BirdInput() {
        inputMove = ClampMagnitude(inputMove, 0.4f, 1.0f);
        inputRotate = ClampMagnitude(inputRotate, 0.4f, 1.0f);
        direction = (transform.forward * inputMove.y + transform.right * inputMove.x + transform.up * inputAltitude) * movementSpeed;
        torqueRotation = (Vector3.up * inputRotate.x + Vector3.forward * -inputRoll + Vector3.right * -inputRotate.y) * torqueSpeed;
    }

    private void ProcessBirdMovement() {
        float aditionalForce = 1;
        if(speedMode) {
            aditionalForce = boostSpeed;
        }

        physics.rigid.AddForce(direction * aditionalForce * Time.deltaTime, ForceMode.VelocityChange);
        physics.rigid.AddRelativeTorque(torqueRotation * Time.deltaTime, ForceMode.VelocityChange);

        if(physics.rigid.velocity.magnitude <= exitBoostModeTrigger && canCheckBoostModeStatus) {
            speedMode = false;
        }
    }

    private void ProcessCameraMovement() {
        Vector3 offsetPosition = transform.position + (transform.up * cameraOffset.y);
        Vector3 camPos = offsetPosition - cam.forward * cameraOffset.z;
        cam.rotation = Quaternion.Slerp(cam.rotation, transform.rotation, cameraRotationSpeed * Time.fixedDeltaTime);
        cam.position = Vector3.Lerp(cam.position, camPos, cameraFollowSpeed * Time.fixedDeltaTime);
    }

    private void Boost() {
        if(boostTimer >= boostCooldown) {
            physics.rigid.AddForce(transform.forward * boostForce, ForceMode.VelocityChange);
            StartCoroutine(CheckBoostModeCooldown());
            speedMode = true;
            boostTimer = 0;
        }
    }

    private IEnumerator CheckBoostModeCooldown() {
        canCheckBoostModeStatus = false;
        yield return new WaitForSeconds(0.5f);
        canCheckBoostModeStatus = true;
    }

    //The code below is for manage the in and out of the bird control state

    //The player will call this method in normal character mode
    public override void Interact() {
        EnterPilotMode();
    }

    //This will hide the player and enter the "Bird Mode" to control the vehicle
    public void EnterPilotMode() {
        if(!PlayerController.Instance.canMove && PlayerController.Instance.reading) return;
        StartCoroutine(EnterPilotModeCoroutine());
    }

    //This method can only be called in the bird mode
    public void ExitPilotMode() {
        if(!piloting) return;
        StartCoroutine(ExitPilotModeCoroutine());
    }

    private IEnumerator EnterPilotModeCoroutine() {
        yield return new WaitForSeconds(changeModeDuration); 
        piloting = true;
        PlayerController.Instance.inputs.Disable();
        inputs.Enable();
        player.SetParent(transform);
        player.gameObject.SetActive(false);
        physics.isActive = false;
        physics.rigid.freezeRotation = true;
    }

    private IEnumerator ExitPilotModeCoroutine() {
        yield return new WaitForSeconds(changeModeDuration); 
        piloting = false;
        player.gameObject.SetActive(true);
        player.SetParent(null);
        PlayerController.Instance.inputs.Enable();
        inputs.Disable();
        physics.isActive = true;
        physics.rigid.freezeRotation = false;
    }
    
    //Input process logic
    private Vector2 ClampMagnitude(Vector2 vector, float minMagnitude, float maxMagnitude)
    {
        float magnitude = Mathf.Clamp(vector.magnitude, minMagnitude, maxMagnitude);
        return vector.normalized * magnitude;
    }
}
