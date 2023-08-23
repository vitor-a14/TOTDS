using UnityEngine;

public class FeetIKHandler : MonoBehaviour
{
    public static FeetIKHandler Instance { get; private set; }
    
    [SerializeField] private Vector3 footIkOffset;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private float ikSpeed;
    [SerializeField] private float ikWeightSpeed;

    public float normalRootYMin, slopeRootYMin;
    public float currentRootYHeight;

    [Range(0, 1)] public float weight;

    private Animator anim;

    private float rightCurrentWeight; 
    private float leftCurrentWeight;

    private Vector3 currentRootPos;

    private Vector3 lLegCurrentPos;
    private Vector3 rLegCurrentPos;
    private Quaternion lLegCurrentRot;
    private Quaternion rLegCurrentRot;

    private void Awake() {
        //Setup instance to be called from other scripts
        if (Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
    }

    private void Start() {
        anim = GetComponent<Animator>();
        currentRootPos = transform.localPosition;
    }

    private void OnAnimatorIK(int layerIndex) {
        Transform leftFootTransf = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
        Transform rightFootTransf = anim.GetBoneTransform(HumanBodyBones.RightFoot);

        Vector3 leftFootPos = leftFootTransf.position;
        Vector3 rightFootPos = rightFootTransf.position;

        RaycastHit lFootHit = GetHitPoint(leftFootPos + Vector3.up, leftFootPos - Vector3.up * 5);
        RaycastHit rFootHit = GetHitPoint(rightFootPos + Vector3.up, rightFootPos - Vector3.up * 5);

        leftFootPos = lFootHit.point + footIkOffset;
        rightFootPos = rFootHit.point + footIkOffset;

        Quaternion alignFootL = Quaternion.LookRotation(transform.forward, lFootHit.normal);
        Quaternion alignFootR = Quaternion.LookRotation(transform.forward, rFootHit.normal);

        //vertical root Pos
        var yPosOffset = -Mathf.Abs(leftFootPos.y - rightFootPos.y);
        Vector3 targetRootPos = new Vector3(0, yPosOffset < currentRootYHeight ? currentRootYHeight : yPosOffset, 0);

        currentRootPos = Vector3.Lerp(currentRootPos, targetRootPos, Time.deltaTime * ikSpeed);

        transform.localPosition = currentRootPos;
        SetWeights();

        //pos
        lLegCurrentPos = Vector3.Lerp(lLegCurrentPos, leftFootPos, Time.deltaTime * ikSpeed);
        rLegCurrentPos = Vector3.Lerp(rLegCurrentPos, rightFootPos, Time.deltaTime * ikSpeed);

        anim.SetIKPosition(AvatarIKGoal.LeftFoot, lLegCurrentPos);
        anim.SetIKPosition(AvatarIKGoal.RightFoot, rLegCurrentPos);

        //rot
        lLegCurrentRot = Quaternion.Lerp(lLegCurrentRot, alignFootL, Time.deltaTime * ikSpeed);
        rLegCurrentRot = Quaternion.Lerp(rLegCurrentRot, alignFootR, Time.deltaTime * ikSpeed);

        anim.SetIKRotation(AvatarIKGoal.LeftFoot, lLegCurrentRot);
        anim.SetIKRotation(AvatarIKGoal.RightFoot, rLegCurrentRot);
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
        RaycastHit hit;
        return Physics.Linecast(start, end, out hit, layerMask) ? hit : hit;
    }
}
