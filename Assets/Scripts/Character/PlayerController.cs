using System.Collections;
using UnityEngine;

public class PlayerController : PhysicsObject
{
    public static PlayerController Instance { get; private set; }

    [HideInInspector] public Inputs inputs;

    [Header("Movement Settings")]
    public bool canMove = true;
    public Transform cam;
    public float movementSpeed;
    public float rigidDrag;
    public Transform characterModel;

    [Header("Jump Settings")]
    public float jumpForce;
    public float aditionalJumpForce;
    public float groundDistanceCheck;
    public LayerMask walkableLayers;
    public float holdJumpTime;

    private float jumpingTimer;
    private Vector2 input;
    private Vector3 processedDirection;
    private Vector3 surfaceNormal;
    [HideInInspector] public Vector3 direction;
    public bool jumping;

    private bool _onGround = false;
    //To detect if the player hit the ground and activate a callback to the animation
    public bool onGround 
    {
        get { return _onGround; }
        set
        {
            if (_onGround != value && value == true) {
                _onGround = value;
                StartCoroutine(CharacterAnimation.Instance.LandingWindow());
            }
            else {
                _onGround = value;
            }
        }
    }

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
        if(!canMove) return;

        Vector3 gravityDirection = GetGravityDirection();
        Vector3 forward = Vector3.Cross(-gravityDirection, cam.right).normalized;
        Vector3 right = Vector3.Cross(-gravityDirection, -cam.forward).normalized;

        input = Vector2.ClampMagnitude(input, 1f);
        direction = (forward * input.y + right * input.x) * movementSpeed;

        if (input != Vector2.zero) {
            CharacterAnimation.Instance.landing = false;
            Quaternion modelRotation = Quaternion.LookRotation(-direction.normalized, gravityDirection);
            characterModel.rotation = Quaternion.Slerp(characterModel.rotation, modelRotation, 15f * Time.deltaTime);
        }

        CheckGround();
    }

    private void FixedUpdate() {
        UpdatePhysics();
        ApplyMotion();
        JumpAditionalForce();
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

    private void CheckGround() {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 0.1f, -transform.up, out hit, groundDistanceCheck, walkableLayers)) {
            surfaceNormal = hit.normal;
            onGround = true;
        } else {
            onGround = false;
        }
    }

    //If the player is holding the jump button, the character will jump higher
    private void JumpAditionalForce() {
        if(!jumping) return;

        if(jumpingTimer < holdJumpTime) {
            jumpingTimer += Time.deltaTime;
            rigid.AddForce(transform.up * aditionalJumpForce, ForceMode.Acceleration);
        }
    }

    private void Jump() {
        if (onGround) {
            rigid.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
            rigid.drag = 0f;
            jumpingTimer = 0f;
            StartCoroutine(JumpWindow());
        }
    }

    //Time window to know if the player is pressing the jumping button and apply a higher jump force
    private IEnumerator JumpWindow() {
        jumping = true;
        yield return new WaitForSeconds(1f);
        jumping = false;
    }  
}
