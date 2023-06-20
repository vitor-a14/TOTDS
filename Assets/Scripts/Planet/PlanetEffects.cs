using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;

public class PlanetEffects : MonoBehaviour
{
	public UniversalRendererData renderData;
	private Blit blit;

	[Header("Atmosphere Optical Depth Texture")]
	public RenderTexture opticalDepthTexture;
	public ComputeShader opticalDepthCompute;
	public int textureSize = 256;
	public int opticalDepthPoints = 7;
	public float densityFalloff = 8;
	public float atmosphereScale = 1;

    void Awake() {
        PrecomputeOutScattering();
    }

    void PrecomputeOutScattering () {
        CreateRenderTexture (ref opticalDepthTexture, textureSize, FilterMode.Bilinear);
        opticalDepthCompute.SetTexture (0, "Result", opticalDepthTexture);
        opticalDepthCompute.SetInt ("textureSize", textureSize);
        opticalDepthCompute.SetInt ("numOutScatteringSteps", opticalDepthPoints);
        opticalDepthCompute.SetFloat ("atmosphereRadius", (1 + atmosphereScale));
        opticalDepthCompute.SetFloat ("densityFalloff", densityFalloff);
        Run (opticalDepthCompute, textureSize, textureSize);

        RenderTexture.active = opticalDepthTexture;
        Texture2D tex = new Texture2D(opticalDepthTexture.width, opticalDepthTexture.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, opticalDepthTexture.width, opticalDepthTexture.height), 0, 0);
        RenderTexture.active = null;

        byte[] bytes;
        bytes = tex.EncodeToPNG();
        
        string path = "Assets/Textures/OpticalDepthTexture.png";
        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path);
	}

	private static void CreateRenderTexture (ref RenderTexture texture, int size, FilterMode filterMode = FilterMode.Bilinear, GraphicsFormat format = GraphicsFormat.R16G16B16A16_SFloat) {
		CreateRenderTexture (ref texture, size, size, filterMode, format);
	}

	private static void CreateRenderTexture (ref RenderTexture texture, int width, int height, FilterMode filterMode = FilterMode.Bilinear, GraphicsFormat format = GraphicsFormat.R16G16B16A16_SFloat) {
		if (texture == null || !texture.IsCreated () || texture.width != width || texture.height != height || texture.graphicsFormat != format) {
			if (texture != null) {
				texture.Release ();
			}
			texture = new RenderTexture (width, height, 0);
			texture.graphicsFormat = format;
			texture.enableRandomWrite = true;

			texture.autoGenerateMips = false;
			texture.Create ();
		}
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.filterMode = filterMode;
	}

	private static void Run (ComputeShader cs, int numIterationsX, int numIterationsY = 1, int numIterationsZ = 1, int kernelIndex = 0) {
		Vector3Int threadGroupSizes = GetThreadGroupSizes (cs, kernelIndex);
		int numGroupsX = Mathf.CeilToInt (numIterationsX / (float) threadGroupSizes.x);
		int numGroupsY = Mathf.CeilToInt (numIterationsY / (float) threadGroupSizes.y);
		int numGroupsZ = Mathf.CeilToInt (numIterationsZ / (float) threadGroupSizes.y);
		cs.Dispatch (kernelIndex, numGroupsX, numGroupsY, numGroupsZ);
	}

	private static Vector3Int GetThreadGroupSizes (ComputeShader compute, int kernelIndex = 0) {
		uint x, y, z;
		compute.GetKernelThreadGroupSizes (kernelIndex, out x, out y, out z);
		return new Vector3Int ((int) x, (int) y, (int) z);
	}
}
