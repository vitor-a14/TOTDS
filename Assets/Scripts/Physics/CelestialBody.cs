using UnityEngine;

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
    public Transform orbitAround;
    public Transform skyDome;

    [HideInInspector] public Rigidbody rigid;
    private bool playerOnSurface;

    private void Start() {
        rigid = GetComponent<Rigidbody>();
    }

    private void FixedUpdate() {
        if(playerOnSurface) {  
            if(orbitAround != null) {
                float combinedRotationSpeed = rotationSpeed + orbitSpeed;
                orbitAround.RotateAround(transform.position, orbitAround.up, combinedRotationSpeed * Time.fixedDeltaTime);
                skyDome.Rotate(transform.up * rotationSpeed * Time.fixedDeltaTime);
            }
        } else {
            transform.Rotate(transform.up * rotationSpeed * Time.fixedDeltaTime);

            if(orbitAround != null)
                transform.RotateAround(orbitAround.transform.position, transform.up, orbitSpeed * Time.fixedDeltaTime);
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
}
