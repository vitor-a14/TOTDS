using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    private static Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
    [SerializeField] [HideInInspector] MeshFilter[] meshFilters;
    [SerializeField] [HideInInspector] TerrainFace[] terrainFaces;
    [SerializeField] [HideInInspector] public float distanceToPlayer;
    public static float cullingMinAngle = 1.6f;
    public static float renderTick = 0.2f;
    public int resolution = 9;

    public float size = 10f;
    public Material planetMaterial;
    public static Transform target;

    public float[] detailLevelDistances = new float[] {
        Mathf.Infinity,
        6000f,
        2500f,
        1000f,
        400f,
        150f,
        70f,
        30f,
        10f
    };

    void Start()
    {
        target = Camera.main.transform;
        Initialize();
        GenerateMesh();
        StartCoroutine(PlanetGenerationLoop());
    }
    
    private IEnumerator PlanetGenerationLoop() {
        while(true) {
            yield return new WaitForSeconds(renderTick);
            distanceToPlayer = Vector3.Distance(transform.position, target.position);
            UpdateMesh();
        }
    }

    private void Initialize() {
        if(meshFilters == null || meshFilters.Length == 0) 
            meshFilters = new MeshFilter[6];

        if(terrainFaces == null || terrainFaces.Length == 0)
            terrainFaces = new TerrainFace[6];

        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        for(int i = 0; i < 6; i++) {
            if(meshFilters[i] == null) {
                GameObject meshObject = new GameObject("mesh");
                meshObject.transform.parent = transform;

                meshObject.AddComponent<MeshRenderer>().sharedMaterial = planetMaterial;
                meshFilters[i] = meshObject.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
                meshFilters[i].sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, resolution, directions[i], size, this);
        }
    }

    private void GenerateMesh() {
        foreach(TerrainFace face in terrainFaces) {
            face.ConstructTree();
        }
    }

    private void UpdateMesh() {
        foreach(TerrainFace face in terrainFaces) {
            face.UpdateTree();
        }
    }
}
