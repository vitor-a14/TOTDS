using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetMesh : MonoBehaviour
{
    private static Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
    [SerializeField] [HideInInspector] MeshFilter[] meshFilters;
    [SerializeField] [HideInInspector] TerrainFace[] terrainFaces;
    [SerializeField] [HideInInspector] public float distanceToPlayer;
    [SerializeField] [HideInInspector] public float distanceToPlayerPow2;
    public static float cullingMinAngle = 1.45f;
    public static float renderTick = 0.2f;
    public bool generateCollider;

    public Noise heightNoise;
    public float size = 10f;
    public Material planetMaterial;
    public static Transform target;

    public float[] detailLevelDistances = new float[] {
        Mathf.Infinity,
        3000f,
        1100f,
        500f,
        210f,
        100f,
        40f
    };

    private void Awake() {
        target = Camera.main.transform;
        distanceToPlayer = Vector3.Distance(transform.position, target.position);
        distanceToPlayerPow2 = distanceToPlayer * distanceToPlayer;
    }

    void Start() {
        Initialize();
        GenerateMesh();
        StartCoroutine(PlanetGenerationLoop());
    }
    
    private IEnumerator PlanetGenerationLoop() {
        while(true) {
            yield return new WaitForSeconds(renderTick);
            distanceToPlayer = Vector3.Distance(transform.position, target.position);
            distanceToPlayerPow2 = distanceToPlayer * distanceToPlayer;
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
                meshObject.layer = gameObject.layer;
                meshObject.tag = gameObject.tag;
                meshObject.transform.parent = transform;

                meshObject.AddComponent<MeshRenderer>().sharedMaterial = planetMaterial;
                meshFilters[i] = meshObject.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }

            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, directions[i], size, this, meshFilters[i].gameObject.AddComponent<MeshCollider>());
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
