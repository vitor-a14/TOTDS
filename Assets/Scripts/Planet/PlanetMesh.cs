using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class PlanetMesh : MonoBehaviour
{
    [Range(2, 256)] [SerializeField] private int resolution = 16;
	[Range(100, 1500)] [SerializeField] private int radius;
	[SerializeField] private ComputeShader heightShader;

	[SerializeField, HideInInspector] MeshFilter meshFilter;
    [SerializeField, HideInInspector] MeshData meshData;

	private void Awake() {
		GenerateMesh(resolution);
	}

	private void OnValidate() {
        if (!Application.isEditor) return;
        GenerateMesh(resolution);
	}

	private void GenerateMesh(int resolution) {
		meshData = CalculateMesh(resolution);
		meshFilter = GetComponent<MeshFilter>();

		if(meshFilter.sharedMesh == null)
			meshFilter.sharedMesh = new Mesh();

		meshFilter.sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
		meshFilter.sharedMesh.Clear();
		meshFilter.sharedMesh.vertices = meshData.vertices;
		meshFilter.sharedMesh.triangles = meshData.triangles;
		meshFilter.sharedMesh.RecalculateBounds();
		meshFilter.sharedMesh.RecalculateNormals();
		meshFilter.sharedMesh.Optimize();
	}

	private MeshData CalculateMesh(int resolution) {
		MeshGenerator mesh = new MeshGenerator(resolution, radius, heightShader);
		MeshData data = new MeshData();
		data.vertices = mesh.Vertices; //calculate height here
		data.triangles = mesh.Triangles;

		return data;
	}

    private struct MeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
    }
}
