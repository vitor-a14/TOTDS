using UnityEngine;
using System.Collections.Generic;

public enum PhysicsPerspective 
{
    PLANET_SURFACE,
    SPACE
};

public class CelestialBody : MonoBehaviour
{
    public float planetRadius;
    public float rotationSpeed;
    public float orbitSpeed;
    public CelestialBody orbitAround;
    public Transform skyDome;
    public bool isMoon;
    [HideInInspector] public Rigidbody rigid;

    private bool playerOnSurface;
    private Vector3 celestialPosition;
    protected bool canOrbit = true;
    protected List<CelestialBody> children = new List<CelestialBody>();

    private void Start() {
        rigid = GetComponent<Rigidbody>();
        celestialPosition = transform.position;
        if(orbitAround != null) {
            orbitAround.children.Add(this);
        }
    }

    private void FixedUpdate() {
        if(playerOnSurface) {
            canOrbit = true;  
            if(orbitAround != null) {
                float combinedRotationSpeed = rotationSpeed + orbitSpeed;
                skyDome.Rotate(transform.up * combinedRotationSpeed * Time.fixedDeltaTime);
                orbitAround.RotateAround(transform.position, orbitAround.transform.up, combinedRotationSpeed * Time.fixedDeltaTime);

                if(isMoon) {    
                    orbitAround.canOrbit = false;
                    orbitAround.orbitAround.RotateAround(transform.position, orbitAround.orbitAround.transform.up, (combinedRotationSpeed + orbitAround.orbitSpeed) * Time.fixedDeltaTime); //relative orbit o
                    foreach(CelestialBody child in orbitAround.orbitAround.children) {
                        if(child != orbitAround)
                            child.RotateAround(transform.position, child.transform.up, (combinedRotationSpeed + orbitAround.orbitSpeed) * Time.fixedDeltaTime);
                    }
                    foreach(CelestialBody child in orbitAround.children) {
                        if(child != this)
                            child.RotateAround(transform.position, orbitAround.transform.up, combinedRotationSpeed * Time.fixedDeltaTime);
                    }
                } else {
                    foreach(CelestialBody child in orbitAround.children) {
                        child.RotateAround(transform.position, orbitAround.transform.up, combinedRotationSpeed * Time.fixedDeltaTime);
                    }
                }
            }
        } else {
            if(orbitAround != null && canOrbit) {
                RotateAround(orbitAround.transform.position, transform.up, orbitSpeed * Time.fixedDeltaTime);
                foreach(CelestialBody child in children) {
                    child.RotateAround(orbitAround.transform.position, child.transform.up, orbitSpeed * Time.fixedDeltaTime);
                }
            }

            transform.Rotate(transform.up * rotationSpeed * Time.fixedDeltaTime);
        } 
    }

    //this will change the way the planet moves and rotates based on the perspective
    //if the perspective is PLANET_SURFACE, the planet will be static for the player move around withot rigidbody bugs
    //for the illusion of movement, all the movement and rotation that the planet would have will now be faked and applyed to all the other bodies in the space
    //in the other way, if the perspective is SPACE, all the celestial bodies will be commanded to move and rotate as usual
    public void ChangePhysicsPerspective(PhysicsPerspective perspective) {
        if(perspective == PhysicsPerspective.PLANET_SURFACE) {
            foreach(CelestialBody celestialBody in UniversePhysics.Instance.celestialBodies) {
                playerOnSurface = false;
            }

            playerOnSurface = true;
        } else if (perspective == PhysicsPerspective.SPACE) {
            foreach(CelestialBody celestialBody in UniversePhysics.Instance.celestialBodies) {
                playerOnSurface = false;
            }
        }
    }

    //Custom transform rotate around to rotate the body without changing it's rotation 
    private void RotateAround(Vector3 center, Vector3 axis, float angle){
        Vector3 pos = transform.position;
        Quaternion rot = Quaternion.AngleAxis(angle, axis); // get the desired rotation
        Vector3 dir = pos - center; // find current direction relative to center
        dir = rot * dir; // rotate the direction
        transform.position = center + dir; // define new position
    }
}
