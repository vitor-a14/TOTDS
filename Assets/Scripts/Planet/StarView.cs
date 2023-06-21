using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarView : MonoBehaviour
{
    public Transform cam;

    private void FixedUpdate() {
        transform.position = cam.position;
    }
}
