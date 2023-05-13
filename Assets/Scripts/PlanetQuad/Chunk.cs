using System.Collections.Generic;
using UnityEngine;

public class Chunk 
{
    public Planet planet;
    public Chunk[] children;
    public Chunk parent;
    public Vector3 position;
    public float radius;
    public int detailLevel;
    public Vector3 localUp, axisA, axisB;

    public Chunk(Chunk[] children, Chunk parent, Vector3 position, float radius, int detailLevel, Vector3 localUp, Vector3 axisA, Vector3 axisB, Planet planet) {
        this.children = children;
        this.parent = parent;
        this.position = position;
        this.radius = radius;
        this.detailLevel = detailLevel;
        this.localUp = localUp;
        this.axisA = axisA;
        this.axisB = axisB;
        this.planet = planet;
    }

    public void GenerateChildren() {
        if(detailLevel <= 8 && detailLevel >= 0) {
            if(Vector3.Distance(position.normalized * planet.size, Planet.target.position) <= Planet.detailLevelDistances[detailLevel]) {
                children = new Chunk[4];
                children[0] = new Chunk(new Chunk[0], this, position + axisA * radius / 2 + axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB, planet);
                children[1] = new Chunk(new Chunk[0], this, position + axisA * radius / 2 - axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB, planet);
                children[2] = new Chunk(new Chunk[0], this, position - axisA * radius / 2 + axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB, planet);
                children[3] = new Chunk(new Chunk[0], this, position - axisA * radius / 2 - axisB * radius / 2, radius / 2, detailLevel + 1, localUp, axisA, axisB, planet);

                foreach(Chunk child in children) {
                    child.GenerateChildren();
                }
            }
        }
    }

    public Chunk[] GetVisibleChildren() {
        List<Chunk> toBeRendered = new List<Chunk>();
        if(children.Length > 0) {
            foreach(Chunk child in children) {
                toBeRendered.AddRange(child.GetVisibleChildren());
            }
        } else {
            toBeRendered.Add(this);
        }

        return toBeRendered.ToArray();
    }

    //maybe can be put in a compute shader?
    public (Vector3[], int[]) CalculateVerticesAndTriangles(int triangleOffset) {
        int resolution = 8;
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        int triIndex = 0;

        for(int y = 0; y < resolution; y++) {
            for(int x = 0; x < resolution; x++) {
                int i = x + y * resolution;
                Vector2 percent = new Vector2(x, y) / (resolution - 1);
                Vector3 pointOnUnitCube = position + ((percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB) * radius;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized * planet.size;
                vertices[i] = pointOnUnitSphere;

                if(x != resolution - 1 && y != resolution - 1) {
                    triangles[triIndex] = i + triangleOffset;
                    triangles[triIndex + 1] = i + resolution + 1 + triangleOffset;
                    triangles[triIndex + 2] = i + resolution + triangleOffset;
                    triangles[triIndex + 3] = i + triangleOffset;
                    triangles[triIndex + 4] = i + 1 + triangleOffset;
                    triangles[triIndex + 5] = i + resolution + 1 + triangleOffset;
                    triIndex += 6;
                }
            }
        }

        return (vertices, triangles);
    }
}
