using UnityEngine;

public class UniversePhysics : MonoBehaviour
{
    public static UniversePhysics Instance { get; private set; }
    public static float G = 6.6743f * Mathf.Pow(10, 2);

    public CelestialBody[] celestialBodies;

    private void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");

        celestialBodies = GameObject.FindObjectsOfType<CelestialBody>();
    }
}
