using System.Collections;
using UnityEngine;

public class PlayerController : PhysicsObject
{
    public static PlayerController Instance { get; private set; }

    [HideInInspector] public Inputs inputs;

    [Header("Movement Settings")]
    public bool canMove = true;
    public bool reading = false;
    public Transform cam;
    public float movementSpeed;
    public float rigidDrag;
    public Transform characterModel;
    public float inputSmoothDamp;

    [Header("Jump Settings")]
    public float jumpForce;
    public float aditionalJumpForce;
    public float groundDistanceCheck;
    public LayerMask walkableLayers;
    public float holdJumpTime;
    public bool jumping;
    public float jumpCooldown;
    private bool canJump = true;
    private float jumpCooldownCounter = 0;

    [HideInInspector] public string floorTag;
    private bool shiftWalk = false; //only for keyboard
    private float jumpingTimer;
    private Vector2 input;
    [HideInInspector] public Vector2 processedInput;
    private Vector3 processedDirection;
    private Vector3 surfaceNormal;
    [HideInInspector] public Vector3 direction;

    [HideInInspector] public bool nearWall;

    //To detect if the player hit the ground and activate a callback to the animation
    private bool _onGround = true; //only for structure, use the variable below instead
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

    //Instance and input setup
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
        inputs.Character.ShiftWalk.performed += ctx => shiftWalk = true;
        inputs.Character.ShiftWalk.canceled += ctx => shiftWalk = false; 

        inputs.Communication.Up.performed += ctx => CommunicationHandler.Instance.AddInteraction(Interaction.UP);        
        inputs.Communication.Down.performed += ctx => CommunicationHandler.Instance.AddInteraction(Interaction.DOWN);        
        inputs.Communication.Right.performed += ctx => CommunicationHandler.Instance.AddInteraction(Interaction.RIGHT);        
        inputs.Communication.Left.performed += ctx => CommunicationHandler.Instance.AddInteraction(Interaction.LEFT);      
        inputs.Communication.Interact.performed += ctx => InteractHandler();
    }

    //Handle screen message show. If the player is already in reading mode, go to next message until it's gone
    //If player is not in reading mode, show the reading screen
    private void InteractHandler() {
        if(!canMove) return; //validate this line

        if(reading) {
            CommunicationHandler.Instance.readingTarget.ShowMessage();
        } else {
            CommunicationHandler.Instance.InteractWithCurrentTarget();
        }
    }

    private void Start() {
        InitializePhysics();
    }

    private void Update() {
        if(!canMove || reading) {
            direction = Vector3.zero;
            return;
        }

        CheckGround();

        if(onGround) {
            Vector3 gravityDirection = GetGravityDirection();
            Vector3 forward = Vector3.Cross(-gravityDirection, cam.right).normalized;
            Vector3 right = Vector3.Cross(-gravityDirection, -cam.forward).normalized;

            if(shiftWalk)
                processedInput = Vector2.Lerp(processedInput, ClampMagnitude(input, 0f, 0.41f), inputSmoothDamp * Time.deltaTime);
            else 
                processedInput = Vector2.Lerp(processedInput, ClampMagnitude(input, 0f, 1f), inputSmoothDamp * Time.deltaTime);

            direction = (forward * processedInput.y + right * processedInput.x) * movementSpeed;

            if (input != Vector2.zero) {
                CharacterAnimation.Instance.landing = false;
                Quaternion modelRotation = Quaternion.LookRotation(direction.normalized, gravityDirection);
                characterModel.rotation = Quaternion.Slerp(characterModel.rotation, modelRotation, 15f * Time.deltaTime);
            } else {
                Vector3 forwardDir = Vector3.Cross(gravityDirection, characterModel.right);
                Quaternion modelRotation = Quaternion.LookRotation(-forwardDir, gravityDirection);
                characterModel.rotation = Quaternion.Slerp(characterModel.rotation, modelRotation, 15f * Time.deltaTime);
            }
        }
    }

    private void FixedUpdate() {
        UpdatePhysics();
        ApplyMotion();
        JumpAditionalForce();
    }

    private void ApplyMotion() {
        Vector3 startPos = transform.position - transform.up * 0.4f;

        if (onGround) {
            rigid.drag = rigidDrag;
            processedDirection = Vector3.ProjectOnPlane(direction, surfaceNormal);
        } else {
            rigid.drag = 0f;
            processedDirection = direction;
        }

        if(!Physics.Linecast(startPos, startPos + characterModel.forward * 0.02f, walkableLayers) && input.magnitude > 0.4f) {
            rigid.position += processedDirection * Time.fixedDeltaTime;
            nearWall = false;
        } else {
            nearWall = true;
        }
    }

    public void AdjustModelRotation() {
        Vector3 gravityDirection = GetGravityDirection();
        characterModel.rotation = Quaternion.LookRotation(transform.forward, gravityDirection);
    }

    private void CheckGround() {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 0.1f, -transform.up, out hit, groundDistanceCheck, walkableLayers)) {
            surfaceNormal = hit.normal;
            floorTag = hit.collider.transform.tag;
            onGround = true;
            if(!canJump)
                HandleJumpCooldown();
        } else {
            onGround = false;
        }
    }

    //If the player is holding the jump button, the character will jump higher
    private void JumpAditionalForce() {
        if(!jumping || !canMove || reading) return;

        if(jumpingTimer < holdJumpTime) {
            jumpingTimer += Time.fixedDeltaTime;
            rigid.AddForce(transform.up * aditionalJumpForce, ForceMode.Acceleration);
        }
    }

    private void Jump() {
        if(!canMove || reading) return;

        if (onGround && canJump) {
            CharacterAnimation.Instance.PlayJumpAnim();
            rigid.AddForce(transform.up * jumpForce, ForceMode.VelocityChange);
            StartCoroutine(HandleJump());
            canJump = false;
        }
    }

    //Time window to know if the player is pressing the jumping button and apply a higher jump force
    private IEnumerator HandleJump() {
        jumpingTimer = 0f;
        jumping = true;
        yield return new WaitForSeconds(0.6f);
        jumping = false;
    }  

    private void HandleJumpCooldown() {
        if(jumpCooldownCounter >= jumpCooldown) {
            jumpCooldownCounter = 0;
            canJump = true;
        } else {
            jumpCooldownCounter += Time.deltaTime;
        }
    }

    //A custom clamp magnite with min and max. The built in unity ClampMagnitude only has the max parameter
    private Vector2 ClampMagnitude(Vector2 vector, float minMagnitude, float maxMagnitude)
    {
        float magnitude = Mathf.Clamp(vector.magnitude, minMagnitude, maxMagnitude);
        return vector.normalized * magnitude;
    }
}
