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
    public byte detailLevel;
    public byte corner;
    public Vector3 localUp, axisA, axisB;

    public Vector3[] vertices;
    public int[] triangles;

    public byte[] neighbours = new byte[4];

    public Chunk(uint hashvalue, PlanetMesh planet, TerrainFace terrainFace, Chunk[] children, Vector3 position, float radius, byte detailLevel, Vector3 localUp, Vector3 axisA, Vector3 axisB, byte[] neighbours, byte corner) {
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
    }

    public void GenerateChildren() {
        if(detailLevel <= planet.detailLevelDistances.Length - 1 && detailLevel >= 0) {
            if(Vector3.Distance(planet.transform.TransformDirection(position.normalized * planet.size) + planet.transform.position, PlanetMesh.target.position) <= planet.detailLevelDistances[detailLevel]) {
                children = new Chunk[4];
                children[0] = new Chunk(hashvalue * 4, planet, terrainFace, new Chunk[0], position + axisA * radius / 2 - axisB * radius / 2, radius / 2, (byte)(detailLevel+1), localUp, axisA, axisB, new byte[4], 0);
                children[1] = new Chunk(hashvalue * 4 + 1, planet, terrainFace, new Chunk[0], position + axisA * radius / 2 + axisB * radius / 2, radius / 2, (byte)(detailLevel+1), localUp, axisA, axisB, new byte[4], 0);
                children[2] = new Chunk(hashvalue * 4 + 2, planet, terrainFace, new Chunk[0], position - axisA * radius / 2 + axisB * radius / 2, radius / 2, (byte)(detailLevel+1), localUp, axisA, axisB, new byte[4], 0);
                children[3] = new Chunk(hashvalue * 4 + 3, planet, terrainFace, new Chunk[0], position - axisA * radius / 2 - axisB * radius / 2, radius / 2, (byte)(detailLevel+1), localUp, axisA, axisB, new byte[4], 0);

                foreach(Chunk child in children) {
                    child.GenerateChildren();
                }
            }
        }
    }

    public void UpdateChunk() {
        float distanceToPlayer = Vector3.Distance(planet.transform.TransformDirection(position.normalized * planet.size) + planet.transform.position, PlanetMesh.target.position);
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
        neighbours = new byte[4];

        if(corner == 0) { // Top left
            neighbours[1] = CheckNeighbourLOD(1, hashvalue); // West
            neighbours[2] = CheckNeighbourLOD(2, hashvalue); // North
        } else if(corner == 1) { // Top right
            neighbours[0] = CheckNeighbourLOD(0, hashvalue); // East
            neighbours[2] = CheckNeighbourLOD(2, hashvalue); // North
        } else if(corner == 2) { // Bottom right
            neighbours[0] = CheckNeighbourLOD(0, hashvalue); // East
            neighbours[3] = CheckNeighbourLOD(3, hashvalue); // South
        } else if(corner == 3) { // Bottom left
            neighbours[1] = CheckNeighbourLOD(1, hashvalue); // West
            neighbours[3] = CheckNeighbourLOD(3, hashvalue); // South
        }
    }

    private byte CheckNeighbourLOD(byte side, uint hash) {
        uint bitmask = 0;
        byte count = 0;
        uint twoLast;

        while (count < detailLevel * 2) { // 0 through 3 can be represented as a two bit number
            count+=2;
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
        if (terrainFace.parentChunk.GetQuadDetailLevel(hashvalue ^ bitmask, detailLevel) < detailLevel)
            return 1;
        else
            return 0;
    }

    public byte GetQuadDetailLevel(uint querryHash, byte dl) {
        byte dlResult = 0; // dl = detail level

        if (hashvalue == querryHash) {
            dlResult = detailLevel;
        } else {
            if (children.Length > 0)
                dlResult += children[((querryHash >> ((dl - 1) * 2)) & 3)].GetQuadDetailLevel(querryHash, (byte)(dl - 1));
        }

        return dlResult; // Returns 0 if no quad with the given hash is found
    }

    public Chunk[] GetVisibleChildren() {
        List<Chunk> toBeRendered = new List<Chunk>();
        if(children.Length > 0) {
            foreach(Chunk child in children) {
                toBeRendered.AddRange(child.GetVisibleChildren());
            }
        } else {
            if (Mathf.Acos((Mathf.Pow(planet.size, 2) + Mathf.Pow(planet.distanceToPlayer, 2) - 
            Mathf.Pow(Vector3.Distance(planet.transform.TransformDirection(position.normalized * planet.size) + planet.transform.position, PlanetMesh.target.position), 2)) / 
            (2 * planet.size * planet.distanceToPlayer)) < PlanetMesh.cullingMinAngle)
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
        Matrix4x4 transformMatrix;
        Vector3 rotationMatrixAttrib = new Vector3(0,0,0);
        Vector3 flipMatrixAttrib = new Vector3(1,1,1);
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
        triangles = Presets.quadTemplateTriangles[quadIndex];

        for (int i = 0; i < vertices.Length; i++) {
            vertices[i] = transformMatrix.MultiplyPoint(Presets.quadTemplateVertices[quadIndex][i]).normalized * planet.size;
        }

        return (vertices, GetTrianglesWithOffset(triangleOffset));
    }
}
