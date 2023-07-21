using UnityEngine;

public class FeetIKHandler : MonoBehaviour
{
    public static FeetIKHandler Instance { get; private set; }

    public Animator characterAnimation;

    private Vector3 rightFootPosition, leftFootPosition, leftFootIKPosition, rightFootIKPosition;
    private Quaternion leftFootIKRotation, rightFootIKRotation;
    private float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY;

    [SerializeField] [Range(0, 2f)] private float raycastHeightFromGround = 1.14f;
    [SerializeField] [Range(0, 2f)] private float raycastDistance = 1.5f;

    public bool enableFeetIK;
    public bool showDebug = false;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
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

            float variable = Mathf.Lerp(lastFootPositionY, positionIKHolder.y, Time.fixedDeltaTime / 1.0f);
            targetIKPosition.y += variable;
            lastFootPositionY = variable;

            targetIKPosition = transform.TransformPoint(targetIKPosition);
            characterAnimation.SetIKRotation(foot, rotationIKHolder);
        }

        characterAnimation.SetIKPosition(foot, targetIKPosition);
    }

    private void FeetPositionSolver(Vector3 upPosition, ref Vector3 feetIKPositions, ref Quaternion feetIKRotations, Transform foot) {
        RaycastHit hit;
        if(showDebug) {
            Debug.DrawRay(upPosition, -transform.up * (raycastDistance + raycastHeightFromGround), Color.yellow);
        }

        if(Physics.Raycast(upPosition, -transform.up, out hit, raycastDistance + raycastHeightFromGround, PlayerController.Instance.walkableLayers) 
        && PlayerController.Instance.onGround && !PlayerController.Instance.jumping) {
            feetIKPositions = upPosition;
            feetIKPositions = hit.point; 
            feetIKRotations = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            return;
        }

        feetIKPositions = Vector3.zero;
    }

    private void MovePelvisHeight() {
        if(rightFootIKPosition == Vector3.zero || leftFootIKPosition == Vector3.zero || lastPelvisPositionY == 0 || PlayerController.Instance.direction.sqrMagnitude > 0.4f) {
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
        Vector3 newPelvisPosition = relativeBodyPosition + Vector3.up * totalOffset; //my code

        relativeBodyPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, Time.fixedDeltaTime / 0.1f);
        characterAnimation.bodyPosition = transform.TransformPoint(newPelvisPosition);
        lastPelvisPositionY = relativeBodyPosition.y;
    }
}
