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

    public Vector3[] vertices;
    public int[] triangles;

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
        int maxDetail = 8;
        if(detailLevel <= maxDetail && detailLevel >= 0) {
            if(Vector3.Distance(planet.transform.TransformDirection(position.normalized * planet.size) + planet.transform.position, Planet.target.position) <= planet.detailLevelDistances[detailLevel]) {
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

    public void UpdateChunk() {
        float distanceToPlayer = Vector3.Distance(planet.transform.TransformDirection(position.normalized * planet.size) + planet.transform.position, Planet.target.position);
        if (detailLevel <= 8) {
            if (distanceToPlayer > planet.detailLevelDistances[detailLevel]) {
                children = new Chunk[0];
            } else {
                if (children.Length > 0) {
                    foreach (Chunk child in children) {
                        child.UpdateChunk();
                    }
                }
                else {
                    GenerateChildren();
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
            if (Mathf.Acos((Mathf.Pow(planet.size, 2) + Mathf.Pow(planet.distanceToPlayer, 2) - 
            Mathf.Pow(Vector3.Distance(planet.transform.TransformDirection(position.normalized * planet.size) + planet.transform.position, Planet.target.position), 2)) / 
            (2 * planet.size * planet.distanceToPlayer)) < Planet.cullingMinAngle)
            {
                toBeRendered.Add(this);
            }
        }

        return toBeRendered.ToArray();
    }

    public int[] GetTrianglesWithOffset(int triangleOffset) {
        int[] triangles = new int[this.triangles.Length];
        for(int i = 0; i < triangles.Length; i++) {
            triangles[i] = this.triangles[i] + triangleOffset;
        }

        return triangles;
    }

    //maybe can be put in a compute shader?
    public (Vector3[], int[]) CalculateVerticesAndTriangles(int triangleOffset) {
        int resolution = planet.resolution; //resolution of the chunk, MUST BE ODD!
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

                if(x < resolution - 1 && y < resolution - 1) {
                    triangles[triIndex] = i;
                    triangles[triIndex + 1] = i + resolution + 1;
                    triangles[triIndex + 2] = i + resolution;
                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + resolution + 1;
                    triIndex += 6;
                }
            }
        }

        this.vertices = vertices;
        this.triangles = triangles;
        return (vertices, GetTrianglesWithOffset(triangleOffset));
    }
}
