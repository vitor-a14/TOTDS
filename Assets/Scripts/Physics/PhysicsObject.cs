using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsObject : MonoBehaviour
{
    [Header("Physics Settings")]
    public bool isActive = true;

    private Rigidbody rigid;

    private void Start() {
        //Setup the components
        rigid = GetComponent<Rigidbody>();
        if(rigid.useGravity) rigid.useGravity = false;
    }

    private void FixedUpdate() {
        if(!isActive) return;

        foreach(CelestialBody celestialBody in UniversePhysics.Instance.celestialBodies) {
            Vector3 direction = celestialBody.transform.position - transform.position;
            float distance = direction.magnitude;
            float forceMagnitude = UniversePhysics.G * rigid.mass * celestialBody.rigid.mass / Mathf.Pow(distance, 2);
            Vector3 force = direction.normalized * forceMagnitude;

            rigid.AddForce(force);
        }
    }
}
