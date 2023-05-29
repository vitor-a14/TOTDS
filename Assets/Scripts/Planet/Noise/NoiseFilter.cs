using UnityEngine;

public class NoiseFilter : Noise
{
    public static float strength = 0.018f;
    public static int octaves = 4;
    public static float baseRoughness = 3;
    public static float roughness = 4;
    public static float persistance = .5f;
    public static Vector3 center = new Vector3(0f, 0f, 0f);
    public static Noise noise = new Noise();

    public static float CalculateNoise(Vector3 point) {
        float noiseValue = 0;
        float frequency = baseRoughness;
        float amplitude = 1;

        for (int i = 0; i < octaves; i++) {
            float v = noise.Evaluate(point * frequency + center);
            noiseValue += v * amplitude;
            frequency *= roughness;
            amplitude *= persistance;
        }

        float elevation = 1 - Mathf.Abs(noiseValue);
        return elevation * elevation * elevation * strength;
    }
}
