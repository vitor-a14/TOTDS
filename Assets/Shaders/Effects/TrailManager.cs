using UnityEngine;

public class TrailManager : MonoBehaviour
{
    public Vector3 targetPos;
    public float velocity;
    public float angularVelocity;

    private Rigidbody rigid;

    private void Start() {
        rigid = GetComponent<Rigidbody>();
        transform.forward = PlayerController.Instance.transform.up;
        velocity = 5f;
        angularVelocity = 35f;
    }

    private void FixedUpdate() {
        var heading = Quaternion.LookRotation(targetPos - transform.position);

        angularVelocity += angularVelocity * Time.fixedDeltaTime;
        rigid.velocity = transform.forward * velocity;
        rigid.MoveRotation(Quaternion.RotateTowards(transform.rotation, heading, angularVelocity * Time.fixedDeltaTime));

        if(Vector3.Distance(rigid.position, targetPos) < 0.5f) {
            Destroy(gameObject);
        } 
    }
}
