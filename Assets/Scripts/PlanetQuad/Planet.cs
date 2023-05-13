using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    private static Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
    [SerializeField] [HideInInspector] MeshFilter[] meshFilters;
    [SerializeField] [HideInInspector] TerrainFace[] terrainFaces;

    public float size = 10f;
    public Material planetMaterial;
    public static Transform target;

    public static Dictionary<int, float> detailLevelDistances = new Dictionary<int, float>() {
        {0, Mathf.Infinity },
        {1, 60f},
        {2, 25f },
        {3, 10f },
        {4, 4f },
        {5, 1.5f },
        {6, 0.7f },
        {7, 0.3f },
        {8, 0.1f }
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
            yield return new WaitForSeconds(1f);
            GenerateMesh();
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

            terrainFaces[i] = new TerrainFace(meshFilters[i].sharedMesh, 4, directions[i], size, this);
        }
    }

    private void GenerateMesh() {
        foreach(TerrainFace face in terrainFaces) {
            face.ConstructTree();
        }
    }
}
