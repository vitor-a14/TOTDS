using System.Collections.Generic;
using UnityEngine;

public class TerrainFace 
{
    private Mesh mesh;
    private int resolution; 
    private Vector3 localUp, axisA, axisB;
    private float radius;
    private Planet planet;
    private Chunk parentChunk;

    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();

    public TerrainFace(Mesh mesh, int resolution, Vector3 localUp, float radius, Planet planet) {
        this.mesh = mesh;
        this.resolution = resolution;
        this.localUp = localUp;
        this.radius = radius;
        this.planet = planet;

        axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        axisB = Vector3.Cross(localUp, axisA);
    }

    public void ConstructTree() {
        vertices.Clear();
        triangles.Clear();

        parentChunk = new Chunk(null, null, localUp.normalized * planet.size, radius, 0, localUp, axisA, axisB, planet);
        parentChunk.GenerateChildren();

        int triangleOffset = 0;
        foreach(Chunk child in parentChunk.GetVisibleChildren()) {
            (Vector3[], int[]) verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset);
            vertices.AddRange(verticesAndTriangles.Item1);
            triangles.AddRange(verticesAndTriangles.Item2);
            triangleOffset += verticesAndTriangles.Item1.Length;
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        //mesh.RecalculateBounds();
        //mesh.Optimize();
    }

    public void UpdateTree() {
        vertices.Clear();
        triangles.Clear();
        parentChunk.UpdateChunk();

        int triangleOffset = 0;
        foreach(Chunk child in parentChunk.GetVisibleChildren()) {
            (Vector3[], int[]) verticesAndTriangles = (new Vector3[0], new int[0]);
            if (child.vertices == null) {
                verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset);
            } else if(child.vertices.Length == 0) {
                verticesAndTriangles = child.CalculateVerticesAndTriangles(triangleOffset);
            } else {
                verticesAndTriangles = (child.vertices, child.GetTrianglesWithOffset(triangleOffset));
            }

            vertices.AddRange(verticesAndTriangles.Item1);
            triangles.AddRange(verticesAndTriangles.Item2);
            triangleOffset += verticesAndTriangles.Item1.Length;
        }

        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        //mesh.RecalculateBounds();
        //mesh.Optimize();
    }
}


