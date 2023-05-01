using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : PhysicsObject
{
    public static CharacterController Instance { get; private set; }

    [HideInInspector] public Inputs inputs;

    [Header("Movement Settings")]
    public Transform cam;
    public float movementSpeed;
    public float rigidDrag;

    [Header("Jump Settings")]
    public float jumpForce;
    public float groundDistanceCheck;
    public LayerMask walkableLayers;

    private Vector2 input;
    private Vector3 direction, processedDirection;
    private Vector3 surfaceNormal;
    private bool onGround;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");

        inputs = new Inputs();
        inputs.Enable();

        inputs.Character.Movement.performed += ctx => input = ctx.ReadValue<Vector2>();
        inputs.Character.Movement.canceled += ctx => input = Vector2.zero;
        inputs.Character.Jump.performed += ctx => Jump();
    }

    private void Start() {
        InitializePhysics();
    }

    private void Update() {
        Vector3 gravityDirection = GetGravityDirection();
        Vector3 forward = Vector3.Cross(-gravityDirection, cam.right).normalized;
        Vector3 right = Vector3.Cross(-gravityDirection, -cam.forward).normalized;

        input = Vector2.ClampMagnitude(input, 1f);
        direction = (forward * input.y + right * input.x) * movementSpeed;

        /*
        if (input != Vector2.zero)
        {
            Quaternion modelRotation = Quaternion.LookRotation(direction.normalized, -gravityDirection);
            model.rotation = Quaternion.Slerp(model.rotation, modelRotation, 15f * Time.deltaTime);
        } */

        CheckGround();
    }

    private void FixedUpdate() {
        UpdatePhysics();
        ApplyMotion();
    }

    private void ApplyMotion() {
        rigid.MovePosition(rigid.position + processedDirection * Time.deltaTime);
        
        if (onGround) {
            rigid.drag = rigidDrag;
            processedDirection = Vector3.ProjectOnPlane(direction, surfaceNormal);
        } else {
            rigid.drag = 0f;
            processedDirection = direction;
        }
    }

    private void Jump() {
        if (onGround) {
            rigid.drag = 0f;
            rigid.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
            onGround = false;
        }
    }

    private void CheckGround() {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 0.1f, -transform.up, out hit, groundDistanceCheck, walkableLayers)) {
            surfaceNormal = hit.normal;
            onGround = true;
        } else {
            onGround = false;
        }
    }
}
