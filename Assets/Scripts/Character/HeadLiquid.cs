using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadLiquid : MonoBehaviour
{
    public float gravityForceSpeed;

    private void FixedUpdate() {
        LookAtGravityDirection();
    }

    public void LookAtGravityDirection() {
        Vector3 gravityDirection = PlayerController.Instance.GetGravityDirection();

        if(gravityDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(gravityDirection, Vector3.up);
    }
}
