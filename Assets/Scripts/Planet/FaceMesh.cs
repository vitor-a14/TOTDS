using System.Collections.Generic;
using UnityEngine;

public class FaceMesh
{
    public Mesh mesh;
    public Mesh faceCollsionMesh;
    public MeshCollider meshCollider;

    public Vector3 localUp;
    public Vector3 axisA;
    public Vector3 axisB;
    public float radius;
    public Chunk parentChunk;
    public PlanetMesh planetScript;
    public List<Chunk> visibleChildren = new List<Chunk>();

    public List<Vector3> vertices = new List<Vector3>();
    public List<Vector3> normals = new List<Vector3>();
    public List<int> triangles = new List<int>();

    public List<Vector3> colliderVertices = new List<Vector3>();
    public List<int> colliderTriangles = new List<int>();

    public FaceMesh(Mesh mesh, MeshCollider meshCollider, Vector3 localUp, float radius, PlanetMesh planetScript) {
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
        colliderVertices.Clear();
        colliderTriangles.Clear();
        visibleChildren.Clear();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; 

        parentChunk = new Chunk(1, planetScript, this, null, localUp.normalized * planetScript.size, radius, 0, localUp, axisA, axisB, new byte[4], 0);
        parentChunk.GenerateChildren();
        parentChunk.GetVisibleChildren();

        int triangleOffset = 0;
        int colliderTriangleOffset = 0;
        foreach (Chunk child in visibleChildren) {
            child.GetNeighbourLOD();
            child.Calculate();
            vertices.AddRange(child.vertices);
            triangles.AddRange(child.GetTrianglesWithOffset(triangleOffset));
            normals.AddRange(child.normals);
            triangleOffset += child.vertices.Length;

            if(planetScript.proceduralCollision && child.detailLevel >= planetScript.detailLevelDistances.Length) {
                colliderVertices.AddRange(child.vertices);
                colliderTriangles.AddRange(child.GetTrianglesWithOffset(colliderTriangleOffset));
                colliderTriangleOffset += (Presets.quadRes + 1) * (Presets.quadRes + 1);
            }
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

        if(colliderVertices.Count > 0) {
            faceCollsionMesh.Clear();
            faceCollsionMesh.vertices = colliderVertices.ToArray();
            faceCollsionMesh.triangles = colliderTriangles.ToArray();

            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = faceCollsionMesh;
        }
    }

    public void UpdateMesh() {
        vertices.Clear();
        triangles.Clear();
        normals.Clear();
        colliderVertices.Clear();
        colliderTriangles.Clear();
        visibleChildren.Clear();

        parentChunk.UpdateChunk();
        parentChunk.GetVisibleChildren();

        int triangleOffset = 0;
        int colliderTriangleOffset = 0;
        foreach (Chunk child in visibleChildren) {
            child.GetNeighbourLOD();
            child.Calculate();
            vertices.AddRange(child.vertices);
            triangles.AddRange(child.GetTrianglesWithOffset(triangleOffset));
            normals.AddRange(child.normals);
            triangleOffset += (Presets.quadRes + 1) * (Presets.quadRes + 1);

            if(planetScript.proceduralCollision && child.detailLevel >= planetScript.detailLevelDistances.Length) {
                colliderVertices.AddRange(child.vertices);
                colliderTriangles.AddRange(child.GetTrianglesWithOffset(colliderTriangleOffset));
                colliderTriangleOffset += (Presets.quadRes + 1) * (Presets.quadRes + 1);
            }
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

        if(colliderVertices.Count > 0) {
            faceCollsionMesh.Clear();
            faceCollsionMesh.vertices = colliderVertices.ToArray();
            faceCollsionMesh.triangles = colliderTriangles.ToArray();

            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = faceCollsionMesh;
        }
    }
}
