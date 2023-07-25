using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsObject : MonoBehaviour
{
    [Header("Physics Settings")]

    [Tooltip("Use this custom physics script")]
    public bool userGravitacionalForce = true;

    [Tooltip("Rotate the object to the strongest atractor")]
    public bool autoRotate = false;

    [Tooltip("Set the parent to the strongest atractor, useful to follow objects")]
    public bool setParentToPlanet = true; //if activated, the object will be attached to close celestial bodies with SetParent

    public bool isPlayer = false;

    [HideInInspector] public Rigidbody rigid;
    [HideInInspector] public Vector3 mainForceDirection = Vector3.zero;
    [HideInInspector] public CelestialBody lastParentBody;

    private float rotationSpeed = 85f;
    private float setParentThreshold = 300f; //the distance that will make the object be attached to the celestial body

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
        bool celestialBodyNear = false;

        //Go through all attractors and apply force in the object based on the Gravitational Formula
        foreach(CelestialBody celestialBody in UniversePhysics.Instance.celestialBodies) {
            Vector3 direction = celestialBody.transform.position - transform.position;
            float distance = direction.magnitude;
            float forceMagnitude = UniversePhysics.G * rigid.mass * celestialBody.rigid.mass / Mathf.Pow(distance, 2);
            Vector3 force = direction.normalized * forceMagnitude;

            if(force.magnitude > greatestForce.magnitude)
                greatestForce = force;

            if(setParentToPlanet && distance - celestialBody.planetRadius <= setParentThreshold) {
                celestialBodyNear = true;
                if(transform.parent != celestialBody.transform) {
                    transform.SetParent(celestialBody.transform);
                    if(transform.tag == "Player") {
                        celestialBody.ChangePhysicsPerspective(PhysicsPerspective.PLANET_SURFACE);
                        lastParentBody = celestialBody;
                    }
                }
            }

            mainForceDirection = -greatestForce.normalized;

            if(userGravitacionalForce) 
                rigid.AddForce(force);
        }

        if(setParentToPlanet && !celestialBodyNear && transform.parent != null) {
            lastParentBody.ChangePhysicsPerspective(PhysicsPerspective.SPACE);
            lastParentBody = null;
            transform.SetParent(null);
        }

        //Rotate the object towards the greatest force attractor
        if(autoRotate && mainForceDirection != Vector3.zero) {
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, mainForceDirection) * transform.rotation;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    public void SetRotationToGravityDirection() {
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, mainForceDirection) * transform.rotation;
        transform.rotation = targetRotation;
    }

    public Vector3 GetGravityDirection() {
        return mainForceDirection;
    }
}
