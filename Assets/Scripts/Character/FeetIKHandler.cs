using UnityEngine;

public class FeetIKHandler : MonoBehaviour
{
    public static FeetIKHandler Instance { get; private set; }

    public Animator characterAnimation;
    [Range(0, 2f)] public float raycastHeightFromGround = 1.14f;
    [Range(0, 2f)] public float raycastDistance = 1.5f;
    public bool enableFeetIK;

    private Vector3 rightFootPosition, leftFootPosition, leftFootIKPosition, rightFootIKPosition;
    private Quaternion leftFootIKRotation, rightFootIKRotation;
    private float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY;
    private PlayerController player;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
    }

    private void Start() {
        player = PlayerController.Instance;
    }

    private void FixedUpdate() {
        if(!enableFeetIK || characterAnimation == null) return;    

        leftFootPosition = characterAnimation.GetBoneTransform(HumanBodyBones.LeftFoot).transform.position + raycastHeightFromGround * transform.up;
        rightFootPosition = characterAnimation.GetBoneTransform(HumanBodyBones.RightFoot).transform.position + raycastHeightFromGround * transform.up; 
   
        FeetPositionSolver(rightFootPosition, ref rightFootIKPosition, ref rightFootIKRotation, characterAnimation.GetBoneTransform(HumanBodyBones.RightFoot).transform);
        FeetPositionSolver(leftFootPosition, ref leftFootIKPosition, ref leftFootIKRotation, characterAnimation.GetBoneTransform(HumanBodyBones.LeftFoot).transform);
    }

    private void OnAnimatorIK() {
        if(!enableFeetIK || characterAnimation == null) return;     

        MovePelvisHeight();

        characterAnimation.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
        characterAnimation.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);

        characterAnimation.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f - PlayerController.Instance.direction.magnitude);
        characterAnimation.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f - PlayerController.Instance.direction.magnitude);

        MoveFeetToIKPoint(AvatarIKGoal.LeftFoot, leftFootIKPosition, leftFootIKRotation, ref lastLeftFootPositionY);
        MoveFeetToIKPoint(AvatarIKGoal.RightFoot, rightFootIKPosition, rightFootIKRotation, ref lastRightFootPositionY);
    }  

    private void MoveFeetToIKPoint(AvatarIKGoal foot, Vector3 positionIKHolder, Quaternion rotationIKHolder, ref float lastFootPositionY) {
        Vector3 targetIKPosition = characterAnimation.GetIKPosition(foot);

        if(positionIKHolder != Vector3.zero) {
            positionIKHolder = transform.InverseTransformPoint(positionIKHolder); 
            targetIKPosition = transform.InverseTransformPoint(targetIKPosition);

            float variable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, Time.fixedDeltaTime / 0.1f);
            targetIKPosition.y += variable;
            lastFootPositionY = variable;

            targetIKPosition = transform.TransformPoint(targetIKPosition);
            characterAnimation.SetIKRotation(foot, rotationIKHolder);
        }

        characterAnimation.SetIKPosition(foot, targetIKPosition);
    }

    private void FeetPositionSolver(Vector3 upPosition, ref Vector3 feetIKPositions, ref Quaternion feetIKRotations, Transform foot) {
        RaycastHit hit;

        if(Physics.Raycast(upPosition, -transform.up, out hit, raycastDistance + raycastHeightFromGround, player.walkableLayers) && player.onGround) {
            feetIKPositions = upPosition;
            feetIKPositions = hit.point; 
            feetIKRotations = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            return;
        }

        feetIKPositions = Vector3.zero;
    }

    private void MovePelvisHeight() {
        if(rightFootIKPosition == Vector3.zero || leftFootIKPosition == Vector3.zero || lastPelvisPositionY == 0) {
            lastPelvisPositionY = transform.InverseTransformPoint(characterAnimation.bodyPosition).y;
            return;
        }

        Vector3 relativeBodyPosition = transform.InverseTransformPoint(characterAnimation.bodyPosition);
        Vector3 relativeTransform = transform.InverseTransformPoint(transform.position);
        Vector3 relativeLeftFootOffset = transform.InverseTransformPoint(leftFootIKPosition);
        Vector3 relativeRightFootOffset = transform.InverseTransformPoint(rightFootIKPosition);

        float leftOffsetPosition = relativeLeftFootOffset.y - relativeTransform.y;
        float rightOffsetPosition = relativeRightFootOffset.y - relativeTransform.y;

        float totalOffset = (leftOffsetPosition < rightOffsetPosition) ? leftOffsetPosition : rightOffsetPosition;
        Vector3 newPelvisPosition = relativeBodyPosition + Vector3.up * totalOffset; 

        relativeBodyPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, Time.fixedDeltaTime / 0.1f);
        relativeBodyPosition = transform.TransformPoint(relativeBodyPosition);
        characterAnimation.bodyPosition = transform.TransformPoint(newPelvisPosition);
        lastPelvisPositionY = relativeBodyPosition.y;
    }
}
