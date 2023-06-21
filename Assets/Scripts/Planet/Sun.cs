using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
    public Transform camTransform;
    public Light sunLight;

    void FixedUpdate()
    {
        sunLight.transform.LookAt(camTransform.position);
    }
}
