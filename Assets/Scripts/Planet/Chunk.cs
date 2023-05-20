using System.Collections.Generic;
using UnityEngine;

public class Chunk 
{
    public uint hashvalue;
    public TerrainFace terrainFace;
    public PlanetMesh planet;
    public Chunk[] children;
    public Chunk parent;
    public Vector3 position;
    public float radius;
    public int detailLevel;
    public byte corner;
    public Vector3 localUp, axisA, axisB;

    public Vector3 normalizedPos;   
    public Vector3[] vertices;
    public Vector3[] borderVertices;
    public int[] triangles;
    public int[] borderTriangles;
    public Vector3[] normals;

    public byte[] neighbours = new byte[4];

    public Chunk(uint hashvalue, PlanetMesh planet, TerrainFace terrainFace, Chunk[] children, Vector3 position, float radius, int detailLevel, Vector3 localUp, Vector3 axisA, Vector3 axisB, byte[] neighbours, byte corner) {
        this.hashvalue = hashvalue;
        this.planet = planet;
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
        if(detailLevel <= planet.detailLevelDistances.Length - 1 && detailLevel >= 0) {
            if (Vector3.Distance(planet.transform.TransformDirection(normalizedPos * planet.size) + planet.transform.position, PlanetMesh.target.position) <= planet.detailLevelDistances[detailLevel]) {
                children = new Chunk[4];
                children[0] = new Chunk(hashvalue * 4    , planet, terrainFace, new Chunk[0], position + axisA * radius * 0.5f - axisB * radius * 0.5f, radius * 0.5f, detailLevel + 1, localUp, axisA, axisB, new byte[4], 0);
                children[1] = new Chunk(hashvalue * 4 + 1, planet, terrainFace, new Chunk[0], position + axisA * radius * 0.5f + axisB * radius * 0.5f, radius * 0.5f, detailLevel + 1, localUp, axisA, axisB, new byte[4], 0);
                children[2] = new Chunk(hashvalue * 4 + 2, planet, terrainFace, new Chunk[0], position - axisA * radius * 0.5f + axisB * radius * 0.5f, radius * 0.5f, detailLevel + 1, localUp, axisA, axisB, new byte[4], 0);
                children[3] = new Chunk(hashvalue * 4 + 3, planet, terrainFace, new Chunk[0], position - axisA * radius * 0.5f - axisB * radius * 0.5f, radius * 0.5f, detailLevel + 1, localUp, axisA, axisB, new byte[4], 0);

                foreach(Chunk child in children) {
                    child.GenerateChildren();
                }
            }
        }
    }

    public void UpdateChunk() {
        float distanceToPlayer = Vector3.Distance(planet.transform.TransformDirection(normalizedPos * planet.size) + planet.transform.position, PlanetMesh.target.position);
        if (detailLevel <= planet.detailLevelDistances.Length - 1) {
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

    public void GetNeighbourLOD() {
        byte[] newNeighbours = new byte[4];

        if(corner == 0) { // Top left
            newNeighbours[1] = CheckNeighbourLOD(1, hashvalue); // West
            newNeighbours[2] = CheckNeighbourLOD(2, hashvalue); // North
        } else if(corner == 1) { // Top right
            newNeighbours[0] = CheckNeighbourLOD(0, hashvalue); // East
            newNeighbours[2] = CheckNeighbourLOD(2, hashvalue); // North
        } else if(corner == 2) { // Bottom right
            newNeighbours[0] = CheckNeighbourLOD(0, hashvalue); // East
            newNeighbours[3] = CheckNeighbourLOD(3, hashvalue); // South
        } else if(corner == 3) { // Bottom left
            newNeighbours[1] = CheckNeighbourLOD(1, hashvalue); // West
            newNeighbours[3] = CheckNeighbourLOD(3, hashvalue); // South
        }

        neighbours = newNeighbours;
    }

    private byte CheckNeighbourLOD(byte side, uint hash) {
        uint bitmask = 0;
        byte count = 0;
        uint twoLast;

        while (count < detailLevel * 2) { // 0 through 3 can be represented as a two bit number
            count += 2;
            twoLast = (hash & 3); // Get the two last bits of the hash. 0b_10011 --> 0b_11
            bitmask = bitmask * 4; // Add zeroes to the end of the bitmask. 0b_10011 --> 0b_1001100

            // Create mask to get the quad on the opposite side. 2 = 0b_10 and generates the mask 0b_11 which flips it to 1 = 0b_01
            if (side == 2 || side == 3)
                bitmask += 3; // Add 0b_11 to the bitmask
            else
                bitmask += 1; // Add 0b_01 to the bitmask

            // Break if the hash goes in the opposite direction
            if ((side == 0 && (twoLast == 0 || twoLast == 3)) ||
                (side == 1 && (twoLast == 1 || twoLast == 2)) ||
                (side == 2 && (twoLast == 3 || twoLast == 2)) ||
                (side == 3 && (twoLast == 0 || twoLast == 1))) {
                break;
            }

            // Remove already processed bits. 0b_1001100 --> 0b_10011
            hash = hash >> 2;
        }

        // Return 1 (true) if the quad in quadstorage is less detailed
        if (terrainFace.parentChunk.GetNeighbourDetailLevel(hashvalue ^ bitmask, detailLevel) < detailLevel)
            return 1;
        else
            return 0;
    }

    public int GetNeighbourDetailLevel(uint querryHash, int dl) {
        int dlResult = 0; // dl = detail level

        if (hashvalue == querryHash) {
            dlResult = detailLevel;
        } else {
            if (children.Length > 0)
                dlResult += children[((querryHash >> ((dl - 1) * 2)) & 3)].GetNeighbourDetailLevel(querryHash, dl - 1);
        }

        return dlResult; // Returns 0 if no quad with the given hash is found
    }

    public void GetVisibleChildren() {
        if(children.Length > 0) {
            foreach(Chunk child in children) {
                child.GetVisibleChildren();
            }
        } else {
            float b = Vector3.Distance(planet.transform.TransformDirection(normalizedPos * (planet.size - planet.size * 0.1f)) +
                planet.transform.position, PlanetMesh.target.position);

            if (Mathf.Acos(((planet.size * planet.size) + (b * b) -
                planet.distanceToPlayerPow2) / (2 * planet.size * b)) > PlanetMesh.cullingMinAngle)
            {
                terrainFace.visibleChildren.Add(this);
            }
        }
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

    //maybe can be put in a compute shader?
    public (Vector3[], int[], int[], Vector3[], Vector3[]) CalculateVerticesAndTriangles(int triangleOffset, int borderTriangleOffset) {
        Matrix4x4 transformMatrix;
        Vector3 rotationMatrixAttrib = new Vector3(0, 0, 0);
        Vector3 scaleMatrixAttrib = new Vector3(radius, radius, 1);

        if(terrainFace.localUp == Vector3.forward) {
            rotationMatrixAttrib = new Vector3(0, 0, 180);
        } else if (terrainFace.localUp == Vector3.back) {
            rotationMatrixAttrib = new Vector3(0, 180, 0);
        } else if (terrainFace.localUp == Vector3.right) {
            rotationMatrixAttrib = new Vector3(0, 90, 270);
        } else if(terrainFace.localUp == Vector3.left) {
            rotationMatrixAttrib = new Vector3(0, 270, 270);
        } else if (terrainFace.localUp == Vector3.up) {
            rotationMatrixAttrib = new Vector3(270, 0, 90);
        } else if(terrainFace.localUp == Vector3.down) {
            rotationMatrixAttrib = new Vector3(90, 0, 270);
        }

        transformMatrix = Matrix4x4.TRS(position, Quaternion.Euler(rotationMatrixAttrib), scaleMatrixAttrib);
        int quadIndex = (neighbours[0] | neighbours[1] * 2 | neighbours[2] * 4 | neighbours[3] * 8);
        vertices = new Vector3[(Presets.quadRes + 1) * (Presets.quadRes + 1)];

        for (int i = 0; i < vertices.Length; i++) {
            Vector3 pointOnCube = transformMatrix.MultiplyPoint(Presets.quadTemplateVertices[quadIndex][i]);
            Vector3 pointOnUnitSphere = pointOnCube.normalized;
            float elevation = planet.heightNoise.CalculateNoise(pointOnUnitSphere);
            vertices[i] = pointOnUnitSphere * (1 + elevation) * planet.size;
        }

        borderVertices = new Vector3[Presets.quadTemplateBorderVertices[quadIndex].Length];

        for (int i = 0; i < borderVertices.Length; i++)
        {
            Vector3 pointOnCube = transformMatrix.MultiplyPoint(Presets.quadTemplateBorderVertices[quadIndex][i]);
            Vector3 pointOnUnitSphere = pointOnCube.normalized;
            float elevation = planet.heightNoise.CalculateNoise(pointOnUnitSphere);
            borderVertices[i] = pointOnUnitSphere * (1 + elevation) * planet.size;
        }

        triangles = Presets.quadTemplateTriangles[quadIndex];  
        borderTriangles = Presets.quadTemplateBorderTriangles[quadIndex];
        normals = new Vector3[vertices.Length];

        int triangleCount = triangles.Length / 3;
        int vertexIndexA;
        int vertexIndexB;
        int vertexIndexC;
        Vector3 triangleNormal;
        int[] edgefansIndices = Presets.quadTemplateEdgeIndices[quadIndex];

        for (int i = 0; i < triangleCount; i++) {
            int normalTriangleIndex = i * 3;
            vertexIndexA = triangles[normalTriangleIndex];
            vertexIndexB = triangles[normalTriangleIndex + 1];
            vertexIndexC = triangles[normalTriangleIndex + 2];

            triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

            if (edgefansIndices[vertexIndexA] == 0){
                normals[vertexIndexA] += triangleNormal;
            }
            if (edgefansIndices[vertexIndexB] == 0) {
                normals[vertexIndexB] += triangleNormal;
            }
            if (edgefansIndices[vertexIndexC] == 0) {
                normals[vertexIndexC] += triangleNormal;
            }
        }

        int borderTriangleCount = borderTriangles.Length / 3;

        for (int i = 0; i < borderTriangleCount; i++) {
            int normalTriangleIndex = i * 3;
            vertexIndexA = borderTriangles[normalTriangleIndex];
            vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

            // Apply the normal if the vertex is on the visible edge of the quad
            if (vertexIndexA >= 0 && (vertexIndexA % (Presets.quadRes + 1) == 0 ||
                vertexIndexA % (Presets.quadRes + 1) == Presets.quadRes ||
                (vertexIndexA >= 0 && vertexIndexA <= Presets.quadRes) ||
                (vertexIndexA >= (Presets.quadRes + 1) * Presets.quadRes && vertexIndexA < (Presets.quadRes + 1) * (Presets.quadRes + 1))))
            {
                normals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0 && (vertexIndexB % (Presets.quadRes + 1) == 0 ||
                vertexIndexB % (Presets.quadRes + 1) == Presets.quadRes ||
                (vertexIndexB >= 0 && vertexIndexB <= Presets.quadRes) ||
                (vertexIndexB >= (Presets.quadRes + 1) * Presets.quadRes && vertexIndexB < (Presets.quadRes + 1) * (Presets.quadRes + 1))))
            {
                normals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0 && (vertexIndexC % (Presets.quadRes + 1) == 0 ||
                vertexIndexC % (Presets.quadRes + 1) == Presets.quadRes ||
                (vertexIndexC >= 0 && vertexIndexC <= Presets.quadRes) ||
                (vertexIndexC >= (Presets.quadRes + 1) * Presets.quadRes && vertexIndexC < (Presets.quadRes + 1) * (Presets.quadRes + 1))))
            {
                normals[vertexIndexC] += triangleNormal;
            }
        }

        for (int i = 0; i < normals.Length; i++) {
            normals[i].Normalize();
        }

        return (vertices, GetTrianglesWithOffset(triangleOffset), GetBorderTrianglesWithOffset(borderTriangleOffset, triangleOffset), borderVertices, normals);
    }

    private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC) {
        Vector3 pointA = (indexA < 0) ? borderVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];

        // Get an aproximation of the vertex normal using two other vertices that share the same triangle
        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }
}