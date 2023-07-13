using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarView : MonoBehaviour
{
    public Transform cam;

    private void Start() {
        if(transform.parent != null)
            transform.SetParent(null);
    }

    private void Update() {
        transform.position = cam.position;
    }
}
