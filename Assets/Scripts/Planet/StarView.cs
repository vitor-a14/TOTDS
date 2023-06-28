using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarView : MonoBehaviour
{
    public Transform cam;
    public Camera auxCamera;

    private void Start() {
        auxCamera.cullingMask = 0;
        if(transform.parent != null)
            transform.SetParent(null);
    }

    private void FixedUpdate() {
        transform.position = cam.position;
    }
}
