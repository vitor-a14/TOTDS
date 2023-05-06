using UnityEngine;

public class Settings : MonoBehaviour
{
    public static Settings Instance { get; private set; }
    public int language;

    public void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");
    }
}
