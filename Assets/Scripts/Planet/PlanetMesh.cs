using System.Collections;
using UnityEngine;

public class PlanetMesh : MonoBehaviour {
    private MeshFilter[] meshFilters;
    private MeshCollider[] meshColliders;
    private FaceMesh[] terrainFaces;

    public static float cullingMinAngle = 1.45f;
    private WaitForSeconds timeTick = new WaitForSeconds(0.5f);

    public float size = 1000; 
    public Transform player;
    public Material planetMaterial;
    public bool proceduralCollision;

    [HideInInspector] public float distanceToPlayer;
    [HideInInspector] public float distanceToPlayerPow2;

    float t = 0;

    public float[] detailLevelDistances = new float[] {
        Mathf.Infinity,
        3000f,
        1100f,
        500f,
        210f,
        100f,
        40f,
    };

    private void Awake() {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        distanceToPlayerPow2 = distanceToPlayer * distanceToPlayer;
    }

    private void Start() {
        float startTime = Time.realtimeSinceStartup;
        Initialize();
        GenerateMesh();
        Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
        //StartCoroutine(PlanetGenerationLoop());
    }

    private void LateUpdate() {
        if (t < 0.5f) {
            t += Time.deltaTime;
        } else {
            distanceToPlayer = Vector3.Distance(transform.position, player.position);
            distanceToPlayerPow2 = distanceToPlayer * distanceToPlayer;
            UpdateMesh();
            t = 0f;
        }
    }

    void Initialize() {
        if (meshFilters == null || meshFilters.Length == 0) {
            meshFilters = new MeshFilter[6];
        }

        if (meshColliders == null || meshColliders.Length == 0) {
            meshColliders = new MeshCollider[6];
        }

        terrainFaces = new FaceMesh[6];
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

        for (int i = 0; i < 6; i++) {
            if (meshFilters[i] == null) {
                GameObject meshObject = new GameObject("mesh");
                meshObject.transform.parent = transform;
                meshObject.transform.tag = transform.tag;
                meshObject.layer = gameObject.layer;

                MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = new Material(planetMaterial);
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
                meshFilters[i] = meshObject.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();

                meshColliders[i] = meshObject.AddComponent<MeshCollider>();
            }

            terrainFaces[i] = new FaceMesh(meshFilters[i].sharedMesh, meshColliders[i], directions[i], size, this);
        }
    }

    void GenerateMesh() {
        foreach (FaceMesh face in terrainFaces) {
            face.GenerateMesh();
        }
    }

    void UpdateMesh() {
        foreach (FaceMesh face in terrainFaces) {
            face.UpdateMesh();
        }
    }
}