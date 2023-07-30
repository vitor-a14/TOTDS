using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfter : MonoBehaviour
{
    public bool autoDestroy;
    public float duration;

    void Start() {
        if(autoDestroy) Destroy(gameObject, duration);
    }

    public void HandleDestroy() {
        Destroy(gameObject);
    }
}
