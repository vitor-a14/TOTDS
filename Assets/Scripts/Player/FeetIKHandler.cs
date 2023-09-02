using System;
using Unity.Mathematics;
using UnityEngine;

public class FeetIKHandler : MonoBehaviour
{
    public static FeetIKHandler Instance { get; private set; }
    
    [SerializeField] private float footIkOffset;
    [SerializeField] private float ikSpeed;
    [SerializeField] private float ikWeightSpeed;
    [SerializeField] private float maxHitDistance;

    public float normalRootYMin, slopeRootYMin;
    [HideInInspector] public float currentRootYMin;

    private Animator anim;
    private PlayerController player;

    private float rightCurrentWeight; 
    private float leftCurrentWeight;

    private Vector3 currentRootPos;

    private Vector3 lLegCurrentPos;
    private Vector3 rLegCurrentPos;
    private Quaternion lLegCurrentRot;
    private Quaternion rLegCurrentRot;

    private Vector3 leftFootPos, rightFootPos;
    private Quaternion alignFootL, alignFootR;
    private RaycastHit lFootHit, rFootHit;

    private void Awake() {
        //Setup instance to be called from other scripts
        if (Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
    }

    private void Start() {
        anim = GetComponent<Animator>();
        player = PlayerController.Instance;
        currentRootPos = transform.localPosition;
    }

    private void OnAnimatorIK(int layerIndex) {
        leftFootPos = anim.GetBoneTransform(HumanBodyBones.LeftFoot).position;
        rightFootPos = anim.GetBoneTransform(HumanBodyBones.RightFoot).position;

        lFootHit = GetHitPoint(leftFootPos + transform.up, leftFootPos - transform.up * maxHitDistance);
        rFootHit = GetHitPoint(rightFootPos + transform.up, rightFootPos - transform.up * maxHitDistance);

        leftFootPos = lFootHit.point != Vector3.zero ? lFootHit.point + (lFootHit.normal * footIkOffset) : leftFootPos;
        rightFootPos = rFootHit.point != Vector3.zero ? rFootHit.point + (rFootHit.normal * footIkOffset): rightFootPos;

        alignFootL = Quaternion.FromToRotation(transform.up, lFootHit.normal) * transform.rotation;
        alignFootR = Quaternion.FromToRotation(transform.up, rFootHit.normal) * transform.rotation;

        SetRootPos();
        SetWeights();

        //Set foot position
        lLegCurrentPos = Vector3.Lerp(lLegCurrentPos, leftFootPos, Time.deltaTime * ikSpeed);
        rLegCurrentPos = Vector3.Lerp(rLegCurrentPos, rightFootPos, Time.deltaTime * ikSpeed);

        anim.SetIKPosition(AvatarIKGoal.LeftFoot, lLegCurrentPos);
        anim.SetIKPosition(AvatarIKGoal.RightFoot, rLegCurrentPos);

        //Set foot rotation
        lLegCurrentRot = Quaternion.Lerp(lLegCurrentRot, alignFootL, Time.deltaTime * ikSpeed);
        rLegCurrentRot = Quaternion.Lerp(rLegCurrentRot, alignFootR, Time.deltaTime * ikSpeed);

        anim.SetIKRotation(AvatarIKGoal.LeftFoot, lLegCurrentRot);
        anim.SetIKRotation(AvatarIKGoal.RightFoot, rLegCurrentRot);
    }

    private void SetRootPos() {
        Vector3 relativeLeftFoot = transform.InverseTransformPoint(lLegCurrentPos);
        Vector3 relativeRightFoot = transform.InverseTransformPoint(rLegCurrentPos);

        float footHeightDifference = -Mathf.Abs(relativeLeftFoot.y - relativeRightFoot.y);
        Vector3 targetRootPos = new Vector3(0, footHeightDifference, 0);

        if(player.onSlope && (player.processedInput.sqrMagnitude > 0.2f || footHeightDifference > -0.3f)) {
            targetRootPos = Vector3.zero;
        }

        //Set root position
        currentRootPos = Vector3.Lerp(currentRootPos, targetRootPos, Time.deltaTime * ikSpeed);
        transform.localPosition = currentRootPos;
    }

    private void SetWeights() {
        leftCurrentWeight = Mathf.Lerp(leftCurrentWeight, anim.GetFloat("LeftFootCurve"), Time.deltaTime * ikWeightSpeed);
        rightCurrentWeight = Mathf.Lerp(rightCurrentWeight, anim.GetFloat("RightFootCurve"), Time.deltaTime * ikWeightSpeed);

        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftCurrentWeight);
        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightCurrentWeight);

        anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftCurrentWeight);
        anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightCurrentWeight);
    }

    private RaycastHit GetHitPoint(Vector3 start, Vector3 end) {
        Physics.Linecast(start, end, out RaycastHit hit, player.walkableLayers);
        return hit;
    }
}
