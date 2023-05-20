using System.Collections.Generic;
using UnityEngine;

public class TerrainFace 
{
    public volatile Mesh mesh;
    public volatile Mesh faceCollsionMesh;

    private int resolution; 
    private Vector3 axisA, axisB;
    private float radius;
    private PlanetMesh planet;
    private MeshCollider faceCollider;
    public List<Chunk> visibleChildren = new List<Chunk>();

    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector3> borderVertices = new List<Vector3>();
    public List<Vector3> normals = new List<Vector3>();
    public List<int> borderTriangles = new List<int>();
    public Dictionary<int, bool> edgefanIndex = new Dictionary<int, bool>();

    public List<Vector3> colliderVertices = new List<Vector3>();
    public List<int> colliderTriangles = new List<int>();
    public List<Vector3> colliderBorderVertices = new List<Vector3>();
    public List<Vector3> colliderNormals = new List<Vector3>();
    public List<int> colliderBorderTriangles = new List<int>();
    public Dictionary<int, bool> colliderEdgefanIndex = new Dictionary<int, bool>();

    public Vector3 localUp;
    public Chunk parentChunk;

    public TerrainFace(Mesh mesh, Vector3 localUp, float radius, PlanetMesh planet, MeshCollider faceCollider) {
        this.mesh = mesh;
        this.localUp = localUp;
        this.radius = radius;
        this.planet = planet;
        this.faceCollider = faceCollider;

        this.faceCollsionMesh = new Mesh();
        this.faceCollsionMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        
        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void ConstructTree() {
        vertices.Clear();
        triangles.Clear();
        normals.Clear();
        borderVertices.Clear();
        borderTriangles.Clear();
        visibleChildren.Clear();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        parentChunk = new Chunk(1, planet, this, null, localUp.normalized * planet.size, radius, 0, localUp, axisA, axisB, new byte[4], 0);
        parentChunk.GenerateChildren();

        int triangleOffset = 0;
        int borderTriangleOffset = 0;
        parentChunk.GetVisibleChildren();
        foreach(Chunk child in visibleChildren) {
            child.GetNeighbourLOD();
            (Vector3[], int[], int[], Vector3[], Vector3[]) verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset, borderTriangleOffset);

            vertices.AddRange(verticesAndTriangles.Item1);
            triangles.AddRange(verticesAndTriangles.Item2);
            borderTriangles.AddRange(verticesAndTriangles.Item3);
            borderVertices.AddRange(verticesAndTriangles.Item4);
            normals.AddRange(verticesAndTriangles.Item5);

            triangleOffset += verticesAndTriangles.Item1.Length;
            borderTriangleOffset += verticesAndTriangles.Item4.Length;
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
    }

    public void UpdateTree() {
        vertices.Clear();
        triangles.Clear();
        normals.Clear();
        borderVertices.Clear();
        borderTriangles.Clear();
        visibleChildren.Clear();
        edgefanIndex.Clear();

        parentChunk.UpdateChunk();

        int triangleOffset = 0;
        int borderTriangleOffset = 0;
        parentChunk.GetVisibleChildren();
        foreach(Chunk child in visibleChildren) {
            child.GetNeighbourLOD();
            (Vector3[], int[], int[], Vector3[], Vector3[]) verticesAndTriangles = (new Vector3[0], new int[0], new int[0], new Vector3[0], new Vector3[0]);
            if (child.vertices == null) {
                verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset, borderTriangleOffset);
            } else if (child.vertices.Length == 0 || child.triangles != Presets.quadTemplateTriangles[(child.neighbours[0] | child.neighbours[1] * 2 | child.neighbours[2] * 4 | child.neighbours[3] * 8)]) {
                verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset, borderTriangleOffset);
            } else {
                verticesAndTriangles = (child.vertices, child.GetTrianglesWithOffset(triangleOffset), child.GetBorderTrianglesWithOffset(borderTriangleOffset, triangleOffset), child.borderVertices, child.normals);
            }

            vertices.AddRange(verticesAndTriangles.Item1);
            triangles.AddRange(verticesAndTriangles.Item2);
            borderTriangles.AddRange(verticesAndTriangles.Item3);
            borderVertices.AddRange(verticesAndTriangles.Item4);
            normals.AddRange(verticesAndTriangles.Item5);

            triangleOffset += (Presets.quadRes + 1) * (Presets.quadRes + 1);
            borderTriangleOffset += verticesAndTriangles.Item4.Length;
        }

        Vector2[] uvs = new Vector2[vertices.Count];

        float planetScriptSizeDivide = (1 / planet.size);
        float twoPiDivide = (1 / (2 * Mathf.PI));

        for (int i = 0; i < uvs.Length; i++)
        {
            Vector3 d = vertices[i] * planetScriptSizeDivide;
            float u = 0.5f + Mathf.Atan2(d.z, d.x) * twoPiDivide;
            float v = 0.5f - Mathf.Asin(d.y) / Mathf.PI;

            uvs[i] = new Vector2(u, v);
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs;
    }

    public void UpdateCollisionMesh() {
        colliderVertices.Clear();
        colliderTriangles.Clear();
        colliderNormals.Clear();
        colliderBorderVertices.Clear();
        colliderBorderTriangles.Clear();
        visibleChildren.Clear();
        colliderEdgefanIndex.Clear();

        parentChunk.UpdateChunk();

        int triangleOffset = 0;
        int borderTriangleOffset = 0;
        parentChunk.GetVisibleChildren();
        foreach(Chunk child in visibleChildren) {
            child.GetNeighbourLOD();
            (Vector3[], int[], int[], Vector3[], Vector3[]) verticesAndTriangles = (new Vector3[0], new int[0], new int[0], new Vector3[0], new Vector3[0]);
            if (child.vertices == null) {
                verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset, borderTriangleOffset);
            } else if (child.vertices.Length == 0 || child.triangles != Presets.quadTemplateTriangles[(child.neighbours[0] | child.neighbours[1] * 2 | child.neighbours[2] * 4 | child.neighbours[3] * 8)]) {
                verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset, borderTriangleOffset);
            } else {
                verticesAndTriangles = (child.vertices, child.GetTrianglesWithOffset(triangleOffset), child.GetBorderTrianglesWithOffset(borderTriangleOffset, triangleOffset), child.borderVertices, child.normals);
            }

            if(child.detailLevel >= planet.detailLevelDistances.Length) {
                colliderVertices.AddRange(verticesAndTriangles.Item1);
                colliderTriangles.AddRange(verticesAndTriangles.Item2);
                colliderBorderTriangles.AddRange(verticesAndTriangles.Item3);
                colliderBorderVertices.AddRange(verticesAndTriangles.Item4);

                triangleOffset += (Presets.quadRes + 1) * (Presets.quadRes + 1);
                borderTriangleOffset += verticesAndTriangles.Item4.Length;
            }
        }

        faceCollsionMesh.Clear();
        faceCollsionMesh.vertices = colliderVertices.ToArray();
        faceCollsionMesh.triangles = colliderTriangles.ToArray();

        if(colliderVertices.Count > 0 && planet.generateCollider) {
            faceCollider.sharedMesh = null;
            faceCollider.sharedMesh = faceCollsionMesh;
        }
    }
}

