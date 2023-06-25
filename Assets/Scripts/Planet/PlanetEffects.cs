using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;

public class PlanetEffects : MonoBehaviour
{
	public UniversalRendererData rendererData;
	private Blit atmosphereBlit;
	private Blit oceanBlit;

	public bool hasAtmosphere;
	public bool hasOcean;
	public float planetRadius;
	public Transform sunTransform;

	//atmosphere
	public Shader atmosphereShader;
	public Texture2D blueNoise;
	[Range(0, 1)] public float atmosphereScale;
	public float densityFalloff = 9;
	public int scatteringPoints = 7;
	public int opticalDepthPoints = 7;
	public Vector3 waveLenghts;
	public float scatteringStrength;
	public float ditheringStrength;
	public float ditheringScale;
	public ComputeShader opticalDepthCompute;
	private RenderTexture opticalDepthTexture;
	public int textureSize = 256;

	//ocean
	public Shader oceanShader;
	[Range(0, 1)] public float oceanScale;
	public Color highColor;
	public Color lowColor;
	public float depthMultiplier;
	public float alphaMultiplier;
	public float smoothness;
	public float waveSpeed;
	public float waveStrength;
	public float waveScale;
	public Texture2D waveNormalA;
	public Texture2D waveNormalB;

	// on inspector render
	public void PreviewEffects() {
		SetEffects();
	}

	// create effects in the render feature
	private void SetEffects() {
		PrecomputeOutScattering();

		if(hasAtmosphere) {
			if(atmosphereBlit == null) {
				Material atmosphereMaterial = new Material(atmosphereShader);
				atmosphereBlit = ScriptableObject.CreateInstance<Blit>();
				atmosphereBlit.SetActive(true);
				atmosphereBlit.name = transform.name + " Atmosphere";
				atmosphereBlit.settings.Event = RenderPassEvent.BeforeRenderingPostProcessing;
				atmosphereBlit.settings.blitMaterial = atmosphereMaterial;
				atmosphereBlit.settings.requireDepthNormals = true;
				rendererData.rendererFeatures.Add(atmosphereBlit);
			}

			//materias properties setup
			atmosphereBlit.settings.blitMaterial.SetVector("_PlanetPosition", transform.position);
			atmosphereBlit.settings.blitMaterial.SetTexture("_BlueNoise", blueNoise);
			atmosphereBlit.settings.blitMaterial.SetFloat("_AtmosphereRadius", atmosphereScale);
			atmosphereBlit.settings.blitMaterial.SetFloat("_PlanetRadius", planetRadius);
			atmosphereBlit.settings.blitMaterial.SetFloat("_DensityFalloff", densityFalloff);
			atmosphereBlit.settings.blitMaterial.SetFloat("_ScatteringPoints", scatteringPoints);
			atmosphereBlit.settings.blitMaterial.SetFloat("_OpticalDepthPoints", opticalDepthPoints);
			atmosphereBlit.settings.blitMaterial.SetVector("_WaveLengths", waveLenghts);
			atmosphereBlit.settings.blitMaterial.SetTexture("_OpticalDepthTexture", opticalDepthTexture);
			atmosphereBlit.settings.blitMaterial.SetFloat("_OceanRadius", hasOcean ? (1 + oceanScale) * planetRadius : 0);
			atmosphereBlit.settings.blitMaterial.SetFloat("_ScaterringStrength", scatteringStrength);
			atmosphereBlit.settings.blitMaterial.SetFloat("_DitheringStrength", ditheringStrength);
			atmosphereBlit.settings.blitMaterial.SetFloat("_DitheringScale", ditheringScale);

			Vector3 sunDirection = (sunTransform.position - transform.position).normalized;
			atmosphereBlit.settings.blitMaterial.SetVector("_SunDir", sunDirection);
		} else {
			if(atmosphereBlit != null) {
				rendererData.rendererFeatures.Remove(atmosphereBlit);
				atmosphereBlit.SetActive(false);
			}

			atmosphereBlit = null;
		}

		if(hasOcean) {
			if(oceanBlit == null) {
				Material oceanMaterial = new Material(oceanShader);
				oceanBlit = ScriptableObject.CreateInstance<Blit>();
				oceanBlit.SetActive(true);
				oceanBlit.name = transform.name + " Ocean";
				oceanBlit.settings.Event = RenderPassEvent.AfterRenderingTransparents;
				oceanBlit.settings.blitMaterial = oceanMaterial;
				oceanBlit.settings.requireDepthNormals = true;
				rendererData.rendererFeatures.Add(oceanBlit);
			}

			oceanBlit.settings.blitMaterial.SetVector("_PlanetPosition", transform.position);
			oceanBlit.settings.blitMaterial.SetFloat("_Radius", (1 + oceanScale) * planetRadius);
			oceanBlit.settings.blitMaterial.SetFloat("_PlanetRadius", planetRadius);
			oceanBlit.settings.blitMaterial.SetColor("_ColorA", highColor);
			oceanBlit.settings.blitMaterial.SetColor("_ColorB", lowColor);
			oceanBlit.settings.blitMaterial.SetFloat("_DepthMultiplier", depthMultiplier);
			oceanBlit.settings.blitMaterial.SetFloat("_AlphaMultiplier", alphaMultiplier);
			oceanBlit.settings.blitMaterial.SetFloat("_Smoothness", smoothness);
			oceanBlit.settings.blitMaterial.SetFloat("_WaveSpeed", waveSpeed);
			oceanBlit.settings.blitMaterial.SetFloat("_WaveStrength", waveStrength);
			oceanBlit.settings.blitMaterial.SetFloat("_WaveScale", waveScale);
			oceanBlit.settings.blitMaterial.SetTexture("waveNormalA", waveNormalA);
			oceanBlit.settings.blitMaterial.SetTexture("waveNormalB", waveNormalB);

			Vector3 sunDirection = (sunTransform.position - transform.position).normalized;
			oceanBlit.settings.blitMaterial.SetVector("_SunDir", sunDirection);
		} else {
			if(oceanBlit != null) {
				rendererData.rendererFeatures.Remove(oceanBlit);
				oceanBlit.SetActive(false);
			}

			oceanBlit = null;
		}

		rendererData.SetDirty();
	}

	// for now, just update positions
	private void UpdateEffects() {
		if(hasAtmosphere && atmosphereBlit != null) {
			Vector3 sunDirection = (sunTransform.position - transform.position).normalized;
			atmosphereBlit.settings.blitMaterial.SetVector("_PlanetPosition", transform.position);
			atmosphereBlit.settings.blitMaterial.SetVector("_SunDir", sunDirection);
		}

		if(hasOcean && oceanBlit != null) {
			Vector3 sunDirection = (sunTransform.position - transform.position).normalized;
			oceanBlit.settings.blitMaterial.SetVector("_PlanetPosition", transform.position);
			oceanBlit.settings.blitMaterial.SetVector("_SunDir", sunDirection);
		}
	}

    private void Start() {
		SetEffects();
	}

	private void LateUpdate() {
		UpdateEffects();
	}

    private void PrecomputeOutScattering () {
        CreateRenderTexture (ref opticalDepthTexture, textureSize, FilterMode.Bilinear);
        opticalDepthCompute.SetTexture (0, "Result", opticalDepthTexture);
        opticalDepthCompute.SetInt ("textureSize", textureSize);
        opticalDepthCompute.SetInt ("numOutScatteringSteps", 40);
        opticalDepthCompute.SetFloat ("atmosphereRadius", ((1 + atmosphereScale)));
        opticalDepthCompute.SetFloat ("densityFalloff", densityFalloff);
        Run (opticalDepthCompute, textureSize, textureSize);
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
