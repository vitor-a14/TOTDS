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
    public CelestialBody orbitAround;
    public Transform skyDome;
    public bool isMoon;

    [HideInInspector] public Rigidbody rigid;
    private bool playerOnSurface;
    protected bool canOrbit = true;

    private void Start() {
        rigid = GetComponent<Rigidbody>();
        //ChangePhysicsPerspective(PhysicsPerspective.SPACE);
    }

    private void FixedUpdate() {
        if(playerOnSurface) {  
            if(orbitAround != null) {
                float combinedRotationSpeed = rotationSpeed + orbitSpeed;
                orbitAround.transform.RotateAround(transform.position, orbitAround.transform.up, combinedRotationSpeed * Time.fixedDeltaTime);
                skyDome.Rotate(transform.up * rotationSpeed * Time.fixedDeltaTime);

                if(isMoon) {
                    orbitAround.orbitAround.transform.RotateAround(orbitAround.transform.position, orbitAround.orbitAround.transform.up, orbitAround.orbitSpeed * Time.fixedDeltaTime);
                }
            }
        } else {
            if(orbitAround != null) {
                transform.RotateAround(orbitAround.transform.position, transform.up, orbitSpeed * Time.fixedDeltaTime);
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
                if(celestialBody.orbitAround != null)
                    celestialBody.transform.SetParent(celestialBody.orbitAround.transform);
                else 
                    celestialBody.transform.SetParent(null);

                playerOnSurface = false;
            }
            
            if(isMoon) {
                transform.SetParent(null); //free moon
                orbitAround.transform.SetParent(null); //free planet
                orbitAround.orbitAround.transform.SetParent(orbitAround.transform); //sun becomes parent of planet
            } else {
                transform.SetParent(null);
            }

            playerOnSurface = true;

        } else if (perspective == PhysicsPerspective.SPACE) {
            foreach(CelestialBody celestialBody in UniversePhysics.Instance.celestialBodies) {
                if(celestialBody.orbitAround != null)
                    celestialBody.transform.SetParent(celestialBody.orbitAround.transform);
                else 
                    celestialBody.transform.SetParent(null);

                playerOnSurface = false;
            }
        }
    }
}
