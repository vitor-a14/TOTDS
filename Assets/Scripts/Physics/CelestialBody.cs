using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    [HideInInspector] public Rigidbody rigid;

    private void Start() {
        rigid = GetComponent<Rigidbody>();
    }
}
