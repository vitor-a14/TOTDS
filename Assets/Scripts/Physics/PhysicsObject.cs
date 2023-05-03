using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsObject : MonoBehaviour
{
    [Header("Physics Settings")]
    public bool isActive = true;
    public bool autoRotate = false;
    private float rotationSpeed = 45f;

    protected Rigidbody rigid;
    protected Vector3 mainForceDirection = Vector3.zero;

    private void Start() {
        InitializePhysics();
    }

    private void FixedUpdate() {
        UpdatePhysics();
    }

    protected void InitializePhysics() {
        //Setup the components
        rigid = GetComponent<Rigidbody>();
        if(rigid.useGravity) rigid.useGravity = false;
    }

    protected void UpdatePhysics() {
        //Variable that will store the greatest force attraction
        Vector3 greatestForce = Vector3.zero;

        //Go through all attractors and apply force in the object based on the Gravitational Formula
        foreach(CelestialBody celestialBody in UniversePhysics.Instance.celestialBodies) {
            Vector3 direction = celestialBody.transform.position - transform.position;
            float distance = direction.magnitude;
            float forceMagnitude = UniversePhysics.G * rigid.mass * celestialBody.rigid.mass / Mathf.Pow(distance, 2);
            Vector3 force = direction.normalized * forceMagnitude;

            if(force.magnitude > greatestForce.magnitude)
                greatestForce = force;

            mainForceDirection = -greatestForce.normalized;
            if(!isActive) return;
                rigid.AddForce(force);
        }

        //Rotate the object towards the greatest force attractor
        if(autoRotate && mainForceDirection != Vector3.zero) {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, mainForceDirection) * transform.rotation;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public Vector3 GetGravityDirection() {
        return mainForceDirection;
    }
}
