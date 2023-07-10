using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Settings : MonoBehaviour
{
    public static Settings Instance { get; private set; }
    public UniversalRendererData rendererData;
    public int language;

    public void Awake() {
        if(Instance == null) 
            Instance = this;
        else
            Debug.LogError("Instance failed to setup because is already setted. Something is wrong.");

        ClearRendererFeatures();
    }

    private void OnDisable() {
        ClearRendererFeatures();
    }

    private void ClearRendererFeatures() {
        rendererData.rendererFeatures.RemoveAll(rendererFeature => rendererFeature.GetType() == typeof(Blit));
        rendererData.rendererFeatures.RemoveAll(rendererFeature => rendererFeature == null);
    }
}
