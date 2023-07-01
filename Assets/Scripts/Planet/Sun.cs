using UnityEngine;

public class Sun : MonoBehaviour
{
    public Transform camTransform;
    public Light sunLight;

    //This is in case of the material using the default lit shader
    //The unity direct light will always point to the player
    private void FixedUpdate() {
        if(camTransform != null)
            sunLight.transform.LookAt(camTransform.position);
    }

    //This should be used by all materials, the LightDirection needs to be calculated based on the sun position
    //For Omnidirectional Shader Uility
    private void LateUpdate() {
        Shader.SetGlobalVector("_SunPos", transform.position);
    }

#if UNITY_EDITOR
    private void OnValidate() {
        Shader.SetGlobalVector("_SunPos", transform.position);
    }
#endif
}
