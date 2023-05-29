using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class Chunk
{
    public uint hashvalue; 
    public PlanetMesh planetScript;
    public FaceMesh terrainFace;

    public Chunk[] children;
    public Vector3 position;
    public Vector3 normalizedPos;
    public float radius;
    public int detailLevel;
    public Vector3 localUp;
    public Vector3 axisA;
    public Vector3 axisB;
    public byte corner;

    public Vector3[] vertices;
    public Vector3[] borderVertices;
    public int[] triangles;
    public int[] borderTriangles;
    public Vector3[] normals;

    public byte[] neighbours = new byte[4];

    public NativeArray<Vector3> verticesArray;
    public NativeArray<Vector3> borderVerticesArray;
    public NativeArray<int> trianglesArray;
    public NativeArray<int> borderTrianglesArray;
    public NativeArray<Vector3> normalsArray;
    public JobHandle jobHandle;

    public Chunk(uint hashvalue, PlanetMesh planetScript, FaceMesh terrainFace, Chunk[] children, Vector3 position, float radius, int detailLevel, Vector3 localUp, Vector3 axisA, Vector3 axisB, byte[] neighbours, byte corner) {
        this.hashvalue = hashvalue;
        this.planetScript = planetScript;
        this.terrainFace = terrainFace;
        this.children = children;
        this.position = position;
        this.radius = radius;
        this.detailLevel = detailLevel;
        this.localUp = localUp;
        this.axisA = axisA;
        this.axisB = axisB;
        this.neighbours = neighbours;
        this.corner = corner;
        this.normalizedPos = position.normalized;
    }

    public void GenerateChildren() {
        if (detailLevel <= planetScript.detailLevelDistances.Length - 1 && detailLevel >= 0) {
            if (Vector3.Distance(planetScript.transform.TransformDirection(normalizedPos * planetScript.size) + planetScript.transform.position, planetScript.player.position) <= planetScript.detailLevelDistances[detailLevel]) {
                children = new Chunk[4];
                children[0] = new Chunk(hashvalue * 4, planetScript, terrainFace, new Chunk[0], position + axisA * radius * 0.5f - axisB * radius * 0.5f, radius * 0.5f, detailLevel + 1, localUp, axisA, axisB, new byte[4], 0); // TOP LEFT
                children[1] = new Chunk(hashvalue * 4 + 1, planetScript, terrainFace, new Chunk[0], position + axisA * radius * 0.5f + axisB * radius * 0.5f, radius * 0.5f, detailLevel + 1, localUp, axisA, axisB, new byte[4], 1); // TOP RIGHT
                children[2] = new Chunk(hashvalue * 4 + 2, planetScript, terrainFace, new Chunk[0], position - axisA * radius * 0.5f + axisB * radius * 0.5f, radius * 0.5f, detailLevel + 1, localUp, axisA, axisB, new byte[4], 2); // BOTTOM RIGHT
                children[3] = new Chunk(hashvalue * 4 + 3, planetScript, terrainFace, new Chunk[0], position - axisA * radius * 0.5f - axisB * radius * 0.5f, radius * 0.5f, detailLevel + 1, localUp, axisA, axisB, new byte[4], 3); // BOTTOM LEFT

                foreach (Chunk child in children) {
                    child.GenerateChildren();
                }
            }
        }
    }

    public void UpdateChunk() {
        float distanceToPlayer = Vector3.Distance(planetScript.transform.TransformDirection(normalizedPos * planetScript.size) + planetScript.transform.position, planetScript.player.position);
        if (detailLevel <= planetScript.detailLevelDistances.Length - 1) {
            if (distanceToPlayer > planetScript.detailLevelDistances[detailLevel]) {
                children = new Chunk[0];
            }
            else {
                if (children.Length > 0) {
                    foreach (Chunk child in children) {
                        child.UpdateChunk();
                    }
                } else {
                    GenerateChildren();
                }
            }
        }
    }

    public void GetVisibleChildren() {
        if (children.Length > 0) {
            foreach (Chunk child in children) {
                child.GetVisibleChildren();
            }
        } else {
            float b = Vector3.Distance(planetScript.transform.TransformDirection(normalizedPos * planetScript.size) + planetScript.transform.position, planetScript.player.position);
            if (Mathf.Acos(((planetScript.size * planetScript.size) + (b * b) - planetScript.distanceToPlayerPow2) / (2 * planetScript.size * b)) > PlanetMesh.cullingMinAngle) {
                terrainFace.visibleChildren.Add(this);
            }
        }
    }

    public void GetNeighbourLOD() {
        byte[] newNeighbours = new byte[4];

        if (corner == 0) 
        {
            newNeighbours[1] = CheckNeighbourLOD(1, hashvalue);
            newNeighbours[2] = CheckNeighbourLOD(2, hashvalue);
        } else if (corner == 1) {
            newNeighbours[0] = CheckNeighbourLOD(0, hashvalue);
            newNeighbours[2] = CheckNeighbourLOD(2, hashvalue);
        } else if (corner == 2) {
            newNeighbours[0] = CheckNeighbourLOD(0, hashvalue);
            newNeighbours[3] = CheckNeighbourLOD(3, hashvalue);
        } else if (corner == 3) {
            newNeighbours[1] = CheckNeighbourLOD(1, hashvalue);
            newNeighbours[3] = CheckNeighbourLOD(3, hashvalue);
        }

        neighbours = newNeighbours;
    }

    private byte CheckNeighbourLOD(byte side, uint hash) {
        uint bitmask = 0;
        byte count = 0;
        uint twoLast;

        while (count < detailLevel * 2) {
            count += 2;
            twoLast = (hash & 3); 
            bitmask = bitmask * 4; 

            if (side == 2 || side == 3) {
                bitmask += 3; 
            } else {
                bitmask += 1; 
            }

            if ((side == 0 && (twoLast == 0 || twoLast == 3)) ||
                (side == 1 && (twoLast == 1 || twoLast == 2)) ||
                (side == 2 && (twoLast == 3 || twoLast == 2)) ||
                (side == 3 && (twoLast == 0 || twoLast == 1)))
            {
                break;
            }

            hash = hash >> 2;
        }

        if (terrainFace.parentChunk.GetNeighbourDetailLevel(hashvalue ^ bitmask, detailLevel) < detailLevel) {
            return 1;
        } else {
            return 0;
        }
    }

    public int GetNeighbourDetailLevel(uint querryHash, int dl) {
        int dlResult = 0; 

        if (hashvalue == querryHash) {
            dlResult = detailLevel;
        } else {
            if (children.Length > 0) {
                dlResult += children[((querryHash >> ((dl - 1) * 2)) & 3)].GetNeighbourDetailLevel(querryHash, dl - 1);
            }
        }

        return dlResult; 
    }

    public int[] GetTrianglesWithOffset(int triangleOffset) {
        int[] newTriangles = new int[triangles.Length];

        for (int i = 0; i < triangles.Length; i++) {
            newTriangles[i] = triangles[i] + triangleOffset;
        }

        return newTriangles;
    }

    public int[] GetBorderTrianglesWithOffset(int borderTriangleOffset, int triangleOffset) {
        int[] newBorderTriangles = new int[borderTriangles.Length];

        for (int i = 0; i < borderTriangles.Length; i++) {
            newBorderTriangles[i] = (borderTriangles[i] < 0) ? borderTriangles[i] - borderTriangleOffset : borderTriangles[i] + triangleOffset;
        }

        return newBorderTriangles;
    }

    public void Calculate(int triangleOffset, int borderTriangleOffset) {
        int quadIndex = (neighbours[0] | neighbours[1] * 2 | neighbours[2] * 4 | neighbours[3] * 8);
        verticesArray = new NativeArray<Vector3>((Presets.quadRes + 1) * (Presets.quadRes + 1), Allocator.TempJob);
        borderVerticesArray = new NativeArray<Vector3>(Presets.quadTemplateBorderVertices[quadIndex].Length, Allocator.TempJob);
        trianglesArray = new NativeArray<int>(Presets.quadTemplateTriangles[quadIndex], Allocator.TempJob);
        borderTrianglesArray = new NativeArray<int>(Presets.quadTemplateBorderTriangles[quadIndex], Allocator.TempJob);
        normalsArray = new NativeArray<Vector3>(verticesArray.Length, Allocator.TempJob);

        ChunkJob calculateJob = new ChunkJob
        {
            triangleOffset = triangleOffset,
            borderTriangleOffset = borderTriangleOffset,
            radius = radius,
            planetSize = planetScript.size,
            localUp = localUp,
            position = position,
            quadIndex = quadIndex,
            vertices = verticesArray,
            borderVertices = borderVerticesArray,
            triangles = trianglesArray,
            borderTriangles = borderTrianglesArray,
            normals = normalsArray
        };

        jobHandle = calculateJob.Schedule();
    }

    public (Vector3[], int[], int[], Vector3[], Vector3[]) GetJob(int triangleOffset, int borderTriangleOffset) {
        jobHandle.Complete();
        
        this.vertices = verticesArray.ToArray();
        this.triangles = trianglesArray.ToArray();
        this.borderTriangles = borderTrianglesArray.ToArray();
        this.borderVertices = borderVerticesArray.ToArray();
        this.normals = normalsArray.ToArray();

        Dispose();

        return (vertices, GetTrianglesWithOffset(triangleOffset), GetBorderTrianglesWithOffset(borderTriangleOffset, triangleOffset), borderVertices, normals);
    }

    public void Dispose() {
        verticesArray.Dispose();
        borderVerticesArray.Dispose();
        trianglesArray.Dispose();
        borderTrianglesArray.Dispose();
        normalsArray.Dispose();
    }

    private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC) {
        Vector3 pointA = (indexA < 0) ? borderVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    private void OnDisable() {
        Dispose();
    }
}