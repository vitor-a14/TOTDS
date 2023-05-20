using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    [HideInInspector] public Rigidbody rigid;
    [HideInInspector] public PlanetMesh planetMeshScript;

    private void Start() {
        rigid = GetComponent<Rigidbody>();
        planetMeshScript = GetComponent<PlanetMesh>();
    }
}
