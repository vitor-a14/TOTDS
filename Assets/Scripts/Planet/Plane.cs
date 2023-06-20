using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

public class Plane 
{ 
    public Quadtree quadtree;
    public Mesh mesh;
    public Mesh collisionMesh;
    public MeshCollider meshCollider;
    public Vector3 localUp;
    public Vector3 AxisA;
    public Vector3 AxisB;

    public int collisionDetailLevel;
    public int res;
    public PlanetMesh terrainGenerator;
    public bool complete = false;
    public LOD Lod;

    public NativeArray<float3> verticeListFixed;
    public NativeArray<float3> normalListFixed;
    public NativeArray<int> triangleListFixed;
    public NativeArray<float2> uvListFixed;

    public NativeArray<float3> verticeListFixedCol;
    public NativeArray<float3> normalListFixedCol;
    public NativeArray<int> triangleListFixedCol;

    public Vector2[] uvs;
    public Color[] vertexcolors;
    public List<float3> verticesExtra;
    public List<float3> normalsExtra;

    public Plane[] neighbours;
    public int[] neighborConnectDirection;
    public float planetRadius;

    public Plane(Mesh mesh,Mesh collisionMesh, Vector3 localUp, int res, PlanetMesh t) {
        this.mesh = mesh;
        this.collisionMesh = collisionMesh;
        
        this.localUp = localUp;
        AxisA = new Vector3(localUp.y, localUp.z, localUp.x);
        AxisB = Vector3.Cross(localUp, AxisA);

        neighbours = new Plane[4];
        neighborConnectDirection = new int[4];

        this.planetRadius = t.radius;
        terrainGenerator = t;
        this.res = res;
        this.Lod = t.lod;
        this.collisionDetailLevel = t.collisionMeshResolution;
        GenerateQuadTree();

        uvs = new Vector2[500000];
    }

    public void SetNeighbors(Plane north, Plane south, Plane east, Plane west, int connectNorth, int connectSouth, int connectEast, int connectWest) {
        neighbours[DIRECTION.NORTH] = north;
        neighbours[DIRECTION.SOUTH] = south;
        neighbours[DIRECTION.EAST] = east;
        neighbours[DIRECTION.WEST] = west;
        neighborConnectDirection[DIRECTION.NORTH] = connectNorth;
        neighborConnectDirection[DIRECTION.SOUTH] = connectSouth;
        neighborConnectDirection[DIRECTION.EAST] = connectEast;
        neighborConnectDirection[DIRECTION.WEST] = connectWest;
    }

    public void SetNativeArrays(NativeArray<float3> verticeListFixed, NativeArray<float3> normalListFixed, NativeArray<int> triangleListFixed,NativeArray<float2> uv) {
        this.verticeListFixed = verticeListFixed;
        this.normalListFixed = normalListFixed;
        this.triangleListFixed = triangleListFixed;
        this.uvListFixed = uv;
    }

    public void SetNativeArraysCollider(NativeArray<float3> verticeListFixed, NativeArray<float3> normalListFixed, NativeArray<int> triangleListFixed) {
        this.verticeListFixedCol = verticeListFixed;
        this.normalListFixedCol = normalListFixed;
        this.triangleListFixedCol = triangleListFixed;
    }
    
    public void GenerateQuadTree() {
        quadtree = new Quadtree(8, planetRadius, localUp * planetRadius, localUp, AxisA, AxisB, res, terrainGenerator,this);
        quadtree.GenerateTree();
    }

    public void UpdateQuadTree() {
        quadtree.UpdateTree();
    }

    public void UpdateMesh() {
        mesh.Clear();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verticeListFixed, 0, quadtree.verticeCount);
        mesh.SetIndices(triangleListFixed, 0, quadtree.triangleCount, MeshTopology.Triangles, 0, false, 0);
        mesh.SetNormals(normalListFixed, 0, quadtree.verticeCount);
        mesh.SetUVs(0, uvListFixed, 0, quadtree.verticeCount);
    }

    void SetUVDataEq() {
        verticesExtra = new List<float3>();
        normalsExtra = new List<float3>();

        int triangleCount = quadtree.triangleCount / 3;
        int normalTriangleIndex;
        int vertexIndexA;
        int vertexIndexB;
        int vertexIndexC;
        float3 pointA;
        float3 pointB;
        float3 pointC;

        for (int i = 0; i < quadtree.verticeCount; i++) {
            uvs[i] = GetUVEq(normalListFixed[i]);
        }
        
        int extraVerticeCount = 0;
        for (int i = 0; i < triangleCount; i++) {
            normalTriangleIndex = i * 3;
            vertexIndexA = triangleListFixed[normalTriangleIndex];
            vertexIndexB = triangleListFixed[normalTriangleIndex + 1];
            vertexIndexC = triangleListFixed[normalTriangleIndex + 2];

            pointA = verticeListFixed[vertexIndexA];
            pointB = verticeListFixed[vertexIndexB];
            pointC = verticeListFixed[vertexIndexC];

            float2 uvA = GetUVEq(normalListFixed[vertexIndexA]);
            float2 uvB = GetUVEq(normalListFixed[vertexIndexB]);
            float2 uvC = GetUVEq(normalListFixed[vertexIndexC]);
            float3 nA = normalListFixed[vertexIndexA];
            float3 nB = normalListFixed[vertexIndexB];
            float3 nC = normalListFixed[vertexIndexC];
            float threshold = 0.5f;

            if (nA.z <= 0.001f && nA.z >= -0.001f && nA.x > 0.01f && (uvB.x>threshold || uvC.x>threshold)) {
                verticesExtra.Add(verticeListFixed[vertexIndexA]);
                normalsExtra.Add(normalListFixed[vertexIndexA]);
                triangleListFixed[normalTriangleIndex] = quadtree.verticeCount + extraVerticeCount;
                uvs[quadtree.verticeCount + extraVerticeCount].x = 1.0f;
                uvs[quadtree.verticeCount + extraVerticeCount].y = uvA.y;
                extraVerticeCount += 1;
            }
            
            if (nB.z <= 0.001f && nB.z >= -0.001f && nB.x >= 0.01f && (uvC.x > threshold || uvA.x > threshold)) {
                verticesExtra.Add(verticeListFixed[vertexIndexB]);
                normalsExtra.Add(normalListFixed[vertexIndexB]);
                triangleListFixed[normalTriangleIndex+1] = quadtree.verticeCount + extraVerticeCount;
                uvs[quadtree.verticeCount + extraVerticeCount].x = 1.0f;
                uvs[quadtree.verticeCount + extraVerticeCount].y = uvB.y;
                extraVerticeCount += 1;
            }
            
            if (nC.z <= 0.001f && nC.z >= -0.001f && nC.x >= 0.01f && (uvA.x > threshold || uvB.x > threshold)) {

                verticesExtra.Add(verticeListFixed[vertexIndexC]);
                normalsExtra.Add(normalListFixed[vertexIndexC]);
                triangleListFixed[normalTriangleIndex+2] = quadtree.verticeCount + extraVerticeCount;
                uvs[quadtree.verticeCount + extraVerticeCount].x = 1.0f;
                uvs[quadtree.verticeCount + extraVerticeCount].y = uvC.y;
                extraVerticeCount += 1;
            }
        }
        
        for(int i = 0; i < extraVerticeCount; i++) {
            verticeListFixed[quadtree.verticeCount] = verticesExtra[i];
            normalListFixed[quadtree.verticeCount] = normalsExtra[i];
            quadtree.verticeCount += 1;
        }
    }

    float2 GetUVEq(float3 n) {
        float u = math.atan2(n.z, n.x) / (math.PI * 2.0f); 
        float v = math.acos(-n.y) / math.PI;
        
        u = u % 1;
        if (u < 0 ) {
            u += 1;
        }

        v = v % 1;
        if (v < 0) {
            v += 1;
        }

        if (u > 0.995f || u < 0.005f) {
            if(n.z<0.001f && n.z>=-0.001f)
            u = 0;
        }

        if (n.z < 0.01f && n.z >= -0.01f && n.x<0.01f && n.x >= -0.01f) {
            u = 0.0f;
        }
            
        return new float2(u, v); 
    }

    public void UpdateCollisionMesh() {
        collisionMesh.Clear();

        if (quadtree.verticeCountCollision == 0) {
            return;
        }

        collisionMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        collisionMesh.SetVertices(verticeListFixedCol, 0, quadtree.verticeCountCollision);
        collisionMesh.SetIndices(triangleListFixedCol, 0, quadtree.triangleCountCollision, MeshTopology.Triangles, 0, false, 0);
        collisionMesh.SetNormals(normalListFixedCol, 0, quadtree.verticeCountCollision);
        meshCollider.sharedMesh = collisionMesh;
    }
}