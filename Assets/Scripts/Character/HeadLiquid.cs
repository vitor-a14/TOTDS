using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadLiquid : MonoBehaviour
{
    public float gravityForceSpeed;

    public float maxWobble = 0.03f;
    public float wobbleSpeed = 1f;
    public float recoveryStrength = 1.0f;
    public Renderer materialRenderer;

    private float time = 0.5f;
    private float wobbleAmountX;
    private float wobbleAmountZ;
    private float wobbleAmountToAddX;
    private float wobbleAmountToAddZ;
    private float pulse;
    Vector3 lastPos;
    Vector3 velocity;
    Vector3 lastRot;  
    Vector3 angularVelocity;

    private void FixedUpdate() {
        LookAtGravityDirection();
        SetWobbleValue();
    }

    public void LookAtGravityDirection() {
        Vector3 gravityDirection = PlayerController.Instance.GetGravityDirection();

        if(gravityDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(gravityDirection, Vector3.up);
    }

    public void SetWobbleValue() {
        time += Time.deltaTime;

        // decrease wobble over time
        wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, Time.deltaTime * (recoveryStrength));
        wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, Time.deltaTime * (recoveryStrength));

        // make a sine wave of the decreasing wobble
        pulse = 2 * Mathf.PI * wobbleSpeed;
        wobbleAmountX = wobbleAmountToAddX * Mathf.Sin(pulse * time);
        wobbleAmountZ = wobbleAmountToAddZ * Mathf.Sin(pulse * time);

        // send it to the shader
        materialRenderer.material.SetFloat("_WobbleX", wobbleAmountX);
        materialRenderer.material.SetFloat("_WobbleZ", wobbleAmountZ);

        // velocity
        velocity = (lastPos - transform.position) / Time.deltaTime;
        angularVelocity = transform.rotation.eulerAngles - lastRot;

        // add clamped velocity to wobble
        wobbleAmountToAddX += Mathf.Clamp((velocity.x + (angularVelocity.z * 0.2f)) * maxWobble, -maxWobble, maxWobble);
        wobbleAmountToAddZ += Mathf.Clamp((velocity.z + (angularVelocity.x * 0.2f)) * maxWobble, -maxWobble, maxWobble);

        // keep last position
        lastPos = transform.position;
        lastRot = transform.rotation.eulerAngles;
    }
}
