using UnityEngine;

public class PlanetNoise : Noise
{
    public float strength = 1;
    [Range(1, 8)] public int octaves = 1;
    public float baseRoughness = 1;
    public float roughness = 2;
    public float persistance = .5f;
    public Vector3 center;

    public Texture2D[] heightMap;
    public Texture2D[] normalMap;

    public override float CalculateNoise(Vector3 point) {
        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;

        for (int i = 0; i < octaves; i++) {
            float v = Evaluate(point * frequency + center);
            noiseValue += v * amplitude;
            frequency *= roughness;
            amplitude *= persistance;
        }

        float elevation = 1 - Mathf.Abs(noiseValue);
        return elevation * elevation * elevation * strength;
    }
}
