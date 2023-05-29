using System.Collections.Generic;
using UnityEngine;

public class FaceMesh
{
    public volatile Mesh mesh;
    public volatile Mesh faceCollsionMesh;
    private MeshCollider meshCollider;
    public Vector3 localUp;
    Vector3 axisA;
    Vector3 axisB;
    float radius;
    public Chunk parentChunk;
    public PlanetMesh planetScript;
    public List<Chunk> visibleChildren = new List<Chunk>();

    public List<Vector3> vertices = new List<Vector3>();
    public List<Vector3> borderVertices = new List<Vector3>();
    public List<Vector3> normals = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<int> borderTriangles = new List<int>();
    public Dictionary<int, bool> edgefanIndex = new Dictionary<int, bool>();

    public List<Vector3> colliderVertices = new List<Vector3>();
    public List<int> colliderTriangles = new List<int>();
    public List<Vector3> colliderBorderVertices = new List<Vector3>();
    public List<int> colliderBorderTriangles = new List<int>();

    public FaceMesh(Mesh mesh, Vector3 localUp, float radius, PlanetMesh planetScript, MeshCollider meshCollider) {
        this.mesh = mesh;
        this.localUp = localUp;
        this.radius = radius;
        this.planetScript = planetScript;
        this.meshCollider = meshCollider;

        this.faceCollsionMesh = new Mesh();
        //this.faceCollsionMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void GenerateMesh() {
        vertices.Clear();
        triangles.Clear();
        normals.Clear();
        borderVertices.Clear();
        borderTriangles.Clear();
        visibleChildren.Clear();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; 

        parentChunk = new Chunk(1, planetScript, this, null, localUp.normalized * planetScript.size, radius, 0, localUp, axisA, axisB, new byte[4], 0);
        parentChunk.GenerateChildren();

        int triangleOffset = 0;
        int borderTriangleOffset = 0;
        parentChunk.GetVisibleChildren();

        foreach (Chunk child in visibleChildren) {
            child.GetNeighbourLOD();
            child.Calculate(triangleOffset, borderTriangleOffset);
            triangleOffset += child.verticesArray.Length;
            borderTriangleOffset += child.borderVerticesArray.Length;
        }

        triangleOffset = 0;
        borderTriangleOffset = 0;
        foreach (Chunk child in visibleChildren) {
            child.GetNeighbourLOD();
            (Vector3[], int[], int[], Vector3[], Vector3[]) result = child.GetJob(triangleOffset, borderTriangleOffset);

            vertices.AddRange(result.Item1);
            triangles.AddRange(result.Item2);
            borderTriangles.AddRange(result.Item3);
            borderVertices.AddRange(result.Item4);
            normals.AddRange(result.Item5);
            triangleOffset += result.Item1.Length;
            borderTriangleOffset += result.Item4.Length;
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();

        if(planetScript.proceduralCollision)
            UpdateCollision();
    }

    public void UpdateMesh() {
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
        foreach (Chunk child in visibleChildren) {
            child.GetNeighbourLOD();
            child.Calculate(triangleOffset, borderTriangleOffset);
            triangleOffset += child.verticesArray.Length;
            borderTriangleOffset += child.borderVerticesArray.Length;
        }

        triangleOffset = 0;
        borderTriangleOffset = 0;
        foreach (Chunk child in visibleChildren) {
            child.GetNeighbourLOD();
            (Vector3[], int[], int[], Vector3[], Vector3[]) result = (new Vector3[0], new int[0], new int[0], new Vector3[0], new Vector3[0]);
            if (child.vertices == null) {
                result = child.GetJob(triangleOffset, borderTriangleOffset);
            }
            else if (child.vertices.Length == 0 || child.triangles != Presets.quadTemplateTriangles[(child.neighbours[0] | child.neighbours[1] * 2 | child.neighbours[2] * 4 | child.neighbours[3] * 8)]) {
                result = child.GetJob(triangleOffset, borderTriangleOffset);
            } else {
                result = (child.vertices, child.GetTrianglesWithOffset(triangleOffset), child.GetBorderTrianglesWithOffset(borderTriangleOffset, triangleOffset), child.borderVertices, child.normals);
            }

            vertices.AddRange(result.Item1);
            triangles.AddRange(result.Item2);
            borderTriangles.AddRange(result.Item3);
            borderVertices.AddRange(result.Item4);
            normals.AddRange(result.Item5);

            triangleOffset += (Presets.quadRes + 1) * (Presets.quadRes + 1);
            borderTriangleOffset += result.Item4.Length;
        }

        Vector2[] uvs = new Vector2[vertices.Count];

        float planetScriptSizeDivide = (1 / planetScript.size);
        float twoPiDivide = (1 / (2 * Mathf.PI));

        for (int i = 0; i < uvs.Length; i++) {
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

        UpdateCollision();
    }

    public void UpdateCollision() {
        colliderVertices.Clear();
        colliderTriangles.Clear();
        visibleChildren.Clear();

        parentChunk.UpdateChunk();

        int triangleOffset = 0;
        int borderTriangleOffset = 0;
        parentChunk.GetVisibleChildren();
        foreach (Chunk child in visibleChildren) {
            if(child.detailLevel >= planetScript.detailLevelDistances.Length) {
                child.GetNeighbourLOD();
                child.Calculate(triangleOffset, borderTriangleOffset);
                triangleOffset += child.verticesArray.Length;
                borderTriangleOffset += child.borderVerticesArray.Length;
            }
        }

        triangleOffset = 0;
        borderTriangleOffset = 0;
        foreach (Chunk child in visibleChildren) {
            if(child.detailLevel >= planetScript.detailLevelDistances.Length) {
                child.GetNeighbourLOD();
                (Vector3[], int[], int[], Vector3[], Vector3[]) result = child.GetJob(triangleOffset, borderTriangleOffset);

                colliderVertices.AddRange(result.Item1);
                colliderTriangles.AddRange(result.Item2);

                triangleOffset += (Presets.quadRes + 1) * (Presets.quadRes + 1);
                borderTriangleOffset += result.Item4.Length;
            }
        }

        if(colliderVertices.ToArray().Length > 0) {
            faceCollsionMesh.Clear();
            faceCollsionMesh.vertices = colliderVertices.ToArray();
            faceCollsionMesh.triangles = colliderTriangles.ToArray();

            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = faceCollsionMesh;
        }
    }
}
