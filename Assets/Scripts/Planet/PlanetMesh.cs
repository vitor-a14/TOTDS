using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public enum TerrainGenerationMode {
    HEIGHTMAP_TEXTURE,
    NOISE_CALCULATION
};

public class PlanetMesh : MonoBehaviour
{
    [Header("General Settings")]
    public float radius = 1000;
    public int collisionMeshResolution;
    public float updateInterval = 50;
    public Transform player;
    public Material Material;

    [Range(2, 16)] public int res = 2;
    public float[] range;

    [Header("Terrain Settings")]
    public TerrainGenerationMode terrainGenerationMode;
    public Texture2D heightMap;
    public int heightMapResolution;
    public float heightMapPower = 200;

    public bool useHeightmapTexture;

    private int verticeFixedSize = 100000;  
    private int verticeFixedSizeCol = 75000;
    int triangleFixedSizeCol;
    int triangleFixedSize;
    int maxDetail;

    private NativeArray<float3> verticeListFixed0;
    private NativeArray<int> triangleListFixed0;
    private NativeArray<float3> normalListFixed0;
    private NativeArray<float2> uvCoordinates0;

    private NativeArray<float3> verticeListFixed1;
    private NativeArray<int> triangleListFixed1;
    private NativeArray<float3> normalListFixed1;
    private NativeArray<float2> uvCoordinates1;

    private NativeArray<float3> verticeListFixed2;
    private NativeArray<int> triangleListFixed2;
    private NativeArray<float3> normalListFixed2;
    private NativeArray<float2> uvCoordinates2;

    private NativeArray<float3> verticeListFixed3;
    private NativeArray<int> triangleListFixed3;
    private NativeArray<float3> normalListFixed3;
    private NativeArray<float2> uvCoordinates3;

    private NativeArray<float3> verticeListFixed4;
    private NativeArray<int> triangleListFixed4;
    private NativeArray<float3> normalListFixed4;
    private NativeArray<float2> uvCoordinates4;

    private NativeArray<float3> verticeListFixed5;
    private NativeArray<int> triangleListFixed5;
    private NativeArray<float3> normalListFixed5;
    private NativeArray<float2> uvCoordinates5;

    private NativeArray<float3> verticeListFixedCol0;
    private NativeArray<int> triangleListFixedCol0;
    private NativeArray<float3> normalListFixedCol0;

    private NativeArray<float3> verticeListFixedCol1;
    private NativeArray<int> triangleListFixedCol1;
    private NativeArray<float3> normalListFixedCol1;

    private NativeArray<float3> verticeListFixedCol2;
    private NativeArray<int> triangleListFixedCol2;
    private NativeArray<float3> normalListFixedCol2;

    private NativeArray<float3> verticeListFixedCol3;
    private NativeArray<int> triangleListFixedCol3;
    private NativeArray<float3> normalListFixedCol3;

    private NativeArray<float3> verticeListFixedCol4;
    private NativeArray<int> triangleListFixedCol4;
    private NativeArray<float3> normalListFixedCol4;

    private NativeArray<float3> verticeListFixedCol5;
    private NativeArray<int> triangleListFixedCol5;
    private NativeArray<float3> normalListFixedCol5;

    private NativeArray<float> planetTextureData;
    private NativeArray<QuadTreeNodeJob> leafnodes;

    private int[] tmpTriangleBordered;
    private int[] tmpTriangle;

    private bool updateCollision;

    private int state_counter=0;

    Mesh[] cubeMesh;
    Mesh[] collisionMesh;
    Plane[] cube;
    Vector3 pos;
    int finishedCount = 6;
    int state = 0;
    int2 heightmapDimensions;
    [HideInInspector] public LOD lod;

    [HideInInspector] [SerializeField] List<GameObject> meshChildren;
    [HideInInspector] [SerializeField] MeshFilter[] meshFilters;
    [HideInInspector] [SerializeField] MeshCollider[] meshColliders;

    private void InitializeScript() {
        maxDetail = range.Length;
        lod = new LOD();
      
        Color[] tmp = heightMap.GetPixels(0, 0, heightMapResolution, heightMapResolution);
        planetTextureData = new NativeArray<float>(tmp.Length, Allocator.Persistent);
        int len = tmp.Length;
        for (int i = 0; i < len; i++) {
            planetTextureData[i] = tmp[i].r; //use red channel reduce size of array
        }

        heightmapDimensions.x = heightMap.width;
        heightmapDimensions.y = heightMap.height;
        Resources.UnloadAsset(heightMap);

        triangleFixedSize = (verticeFixedSize / (res * res) )* ((res - 1) * (res - 1) * 6);
        triangleFixedSizeCol = (verticeFixedSizeCol / (res * res)) * ((res - 1) * (res - 1) * 6);
        
        verticeListFixed0 = new NativeArray<float3>(verticeFixedSize, Allocator.Persistent);
        triangleListFixed0 = new NativeArray<int>(triangleFixedSize, Allocator.Persistent);
        normalListFixed0 = new NativeArray<float3>(verticeFixedSize, Allocator.Persistent);
        uvCoordinates0 = new NativeArray<float2>(verticeFixedSize, Allocator.Persistent);

        verticeListFixed1 = new NativeArray<float3>(verticeFixedSize, Allocator.Persistent);
        triangleListFixed1 = new NativeArray<int>(triangleFixedSize, Allocator.Persistent);
        normalListFixed1 = new NativeArray<float3>(verticeFixedSize, Allocator.Persistent);
        uvCoordinates1 = new NativeArray<float2>(verticeFixedSize, Allocator.Persistent);

        verticeListFixed2 = new NativeArray<float3>(verticeFixedSize, Allocator.Persistent);
        triangleListFixed2 = new NativeArray<int>(triangleFixedSize, Allocator.Persistent);
        normalListFixed2 = new NativeArray<float3>(verticeFixedSize, Allocator.Persistent);
        uvCoordinates2 = new NativeArray<float2>(verticeFixedSize, Allocator.Persistent);

        verticeListFixed3 = new NativeArray<float3>(verticeFixedSize, Allocator.Persistent);
        triangleListFixed3 = new NativeArray<int>(triangleFixedSize, Allocator.Persistent);
        normalListFixed3 = new NativeArray<float3>(verticeFixedSize, Allocator.Persistent);
        uvCoordinates3 = new NativeArray<float2>(verticeFixedSize, Allocator.Persistent);

        verticeListFixed4 = new NativeArray<float3>(verticeFixedSize, Allocator.Persistent);
        triangleListFixed4 = new NativeArray<int>(triangleFixedSize, Allocator.Persistent);
        normalListFixed4 = new NativeArray<float3>(verticeFixedSize, Allocator.Persistent);
        uvCoordinates4 = new NativeArray<float2>(verticeFixedSize, Allocator.Persistent);

        verticeListFixed5 = new NativeArray<float3>(verticeFixedSize, Allocator.Persistent);
        triangleListFixed5 = new NativeArray<int>(triangleFixedSize, Allocator.Persistent);
        normalListFixed5 = new NativeArray<float3>(verticeFixedSize, Allocator.Persistent);
        uvCoordinates5 = new NativeArray<float2>(verticeFixedSize, Allocator.Persistent);

        verticeListFixedCol0 = new NativeArray<float3>(verticeFixedSizeCol, Allocator.Persistent);
        triangleListFixedCol0 = new NativeArray<int>(triangleFixedSizeCol, Allocator.Persistent);
        normalListFixedCol0 = new NativeArray<float3>(verticeFixedSizeCol, Allocator.Persistent);

        verticeListFixedCol1 = new NativeArray<float3>(verticeFixedSizeCol, Allocator.Persistent);
        triangleListFixedCol1 = new NativeArray<int>(triangleFixedSizeCol, Allocator.Persistent);
        normalListFixedCol1 = new NativeArray<float3>(verticeFixedSizeCol, Allocator.Persistent);

        verticeListFixedCol2 = new NativeArray<float3>(verticeFixedSizeCol, Allocator.Persistent);
        triangleListFixedCol2 = new NativeArray<int>(triangleFixedSizeCol, Allocator.Persistent);
        normalListFixedCol2 = new NativeArray<float3>(verticeFixedSizeCol, Allocator.Persistent);

        verticeListFixedCol3 = new NativeArray<float3>(verticeFixedSizeCol, Allocator.Persistent);
        triangleListFixedCol3 = new NativeArray<int>(triangleFixedSizeCol, Allocator.Persistent);
        normalListFixedCol3 = new NativeArray<float3>(verticeFixedSizeCol, Allocator.Persistent);

        verticeListFixedCol4 = new NativeArray<float3>(verticeFixedSizeCol, Allocator.Persistent);
        triangleListFixedCol4 = new NativeArray<int>(triangleFixedSizeCol, Allocator.Persistent);
        normalListFixedCol4 = new NativeArray<float3>(verticeFixedSizeCol, Allocator.Persistent);

        verticeListFixedCol5 = new NativeArray<float3>(verticeFixedSizeCol, Allocator.Persistent);
        triangleListFixedCol5 = new NativeArray<int>(triangleFixedSizeCol, Allocator.Persistent);
        normalListFixedCol5 = new NativeArray<float3>(verticeFixedSizeCol, Allocator.Persistent);
    }

    private void Awake() {
        foreach(GameObject mesh in meshChildren) {
            DestroyImmediate(mesh);
        }

        InitializeScript();
    }

    private void Start() {
        CreateMesh();
    }

    public void RenderPreview() {
        foreach(GameObject mesh in meshChildren) {
            DestroyImmediate(mesh);
        }

        InitializeScript();
        CreateMesh();
        OnDestroy();
    }

    private void CreateMesh() {
        cubeMesh = null;
        cube = null;
        meshFilters = null;
        InitMesh();

        //set mesh for render
        cube[0].SetNativeArrays(verticeListFixed0, normalListFixed0, triangleListFixed0, uvCoordinates0);
        cube[1].SetNativeArrays(verticeListFixed1, normalListFixed1, triangleListFixed1, uvCoordinates1);
        cube[2].SetNativeArrays(verticeListFixed2, normalListFixed2, triangleListFixed2, uvCoordinates2);
        cube[3].SetNativeArrays(verticeListFixed3, normalListFixed3, triangleListFixed3, uvCoordinates3);
        cube[4].SetNativeArrays(verticeListFixed4, normalListFixed4, triangleListFixed4, uvCoordinates4);
        cube[5].SetNativeArrays(verticeListFixed5, normalListFixed5, triangleListFixed5, uvCoordinates5);

        //set mesh for collisions
        cube[0].SetNativeArraysCollider(verticeListFixedCol0, normalListFixedCol0, triangleListFixedCol0);
        cube[1].SetNativeArraysCollider(verticeListFixedCol1, normalListFixedCol1, triangleListFixedCol1);
        cube[2].SetNativeArraysCollider(verticeListFixedCol2, normalListFixedCol2, triangleListFixedCol2);
        cube[3].SetNativeArraysCollider(verticeListFixedCol3, normalListFixedCol3, triangleListFixedCol3);
        cube[4].SetNativeArraysCollider(verticeListFixedCol4, normalListFixedCol4, triangleListFixedCol4);
        cube[5].SetNativeArraysCollider(verticeListFixedCol5, normalListFixedCol5, triangleListFixedCol5);

        pos = new Vector3(player.position.x, player.position.y, player.position.z);
        updateCollision = false;
        
        for (int i = 0; i < 6; i++) {
            cube[i].UpdateQuadTree();
            cube[i].quadtree.GetLeafNodes();
            cube[i].quadtree.UpdateEdgeNeighbors();
        }

        CalculateTriangle();
       
        //claculate mesh on start
        for (int i = 0; i < 6; i++) {
            cube[i].quadtree.ConvertJobs();
            JobCalculateTerrain(i,false);
            JobCalculateTerrain(i, true);
            cube[i].UpdateMesh();

        }

        finishedCount = 0;
    }

    private void OnDestroy() {
        verticeListFixed0.Dispose();
        triangleListFixed0.Dispose();
        normalListFixed0.Dispose();
        uvCoordinates0.Dispose();

        verticeListFixed1.Dispose();
        triangleListFixed1.Dispose();
        normalListFixed1.Dispose();
        uvCoordinates1.Dispose();

        verticeListFixed2.Dispose();
        triangleListFixed2.Dispose();
        normalListFixed2.Dispose();
        uvCoordinates2.Dispose();

        verticeListFixed3.Dispose();
        triangleListFixed3.Dispose();
        normalListFixed3.Dispose();
        uvCoordinates3.Dispose();

        verticeListFixed4.Dispose();
        triangleListFixed4.Dispose();
        normalListFixed4.Dispose();
        uvCoordinates4.Dispose();

        verticeListFixed5.Dispose();
        triangleListFixed5.Dispose();
        normalListFixed5.Dispose();
        uvCoordinates5.Dispose();

        verticeListFixedCol0.Dispose();
        triangleListFixedCol0.Dispose();
        normalListFixedCol0.Dispose();
      
        verticeListFixedCol1.Dispose();
        triangleListFixedCol1.Dispose();
        normalListFixedCol1.Dispose();

        verticeListFixedCol2.Dispose();
        triangleListFixedCol2.Dispose();
        normalListFixedCol2.Dispose();

        verticeListFixedCol3.Dispose();
        triangleListFixedCol3.Dispose();
        normalListFixedCol3.Dispose();

        verticeListFixedCol4.Dispose();
        triangleListFixedCol4.Dispose();
        normalListFixedCol4.Dispose();

        verticeListFixedCol5.Dispose();
        triangleListFixedCol5.Dispose();
        normalListFixedCol5.Dispose();

        planetTextureData.Dispose();
    }

    private void Update() {
        if(Vector3.Distance(gameObject.transform.position, player.position) > 2*radius && finishedCount==6) {
            return;
        }

        if (Vector3.Distance(pos, player.position) > updateInterval && finishedCount == 6 ) {
            LOD.cameraPos = player;
            pos = new Vector3(player.position.x, player.position.y, player.position.z);
            finishedCount = 0; 
            state_counter = 0;
            state = 0;
        }

        if (finishedCount < 6) {
            UpdateMesh();
        }
    }

    public void UpdateMesh() {
        cube[state_counter].UpdateMesh();

        if (state == 0) {
            cube[state_counter].UpdateQuadTree();
            state_counter += 1;
            if (state_counter >= 6) {
                state = 1;
                state_counter = 0;
            }
        } else if (state == 1) {
            cube[state_counter].quadtree.GetLeafNodes();
            state_counter += 1;
            if (state_counter >= 6) {
                state = 2;
                state_counter = 0;
            } 
        } else if (state == 2) {
            cube[state_counter].quadtree.FindNeighbors();
            state_counter += 1;
            if (state_counter >= 6) {
                state = 3;
                state_counter = 0;
            }
        } else if (state == 3) {
            cube[state_counter].quadtree.UpdateEdgeNeighbors();
            state_counter += 1;
            if (state_counter >= 6) {
                state = 4;
                state_counter = 0;
            }
        } else if (state == 4) {
            cube[state_counter].quadtree.ConvertJobs();
            state_counter += 1;
            if (state_counter >= 6) {
                state = 5;
                state_counter = 0;
            }
        } else {
            if (updateCollision == false) {
                JobCalculateTerrain(finishedCount, false);
                cube[finishedCount].UpdateMesh();
                updateCollision = true;
            } else {
                JobCalculateTerrain(finishedCount, true);
                updateCollision = false;
                cube[finishedCount].UpdateCollisionMesh();
                finishedCount += 1;
            }
        }
    }
   
    private void JobCalculateTerrain(int cubeIndex,bool collision) {
        NativeArray<int> borderedSizeIndex = new NativeArray<int>((res + 2) * (res + 2), Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        int meshIndex = 0;
        int borderIndex = -1;
        int c = 0;

        for (int i = 0; i < res + 2; i++) {
            for (int j = 0; j < res + 2; j++) {
                if (i == 0 || i == res + 1 || j == 0 || j == res + 1) {
                    borderedSizeIndex[c] = borderIndex;
                    borderIndex -= 1;
                } else {
                    borderedSizeIndex[c] = meshIndex;
                    meshIndex += 1;
                }

                c += 1;
            }
        }

        float2 uvMap;
        NativeArray<float3> verticeList;
        NativeArray<int> triangleList;
        NativeArray<float3> normalList;
        NativeArray<float2> uvList;

        if (cubeIndex == 0) {
            uvList = uvCoordinates0;
            uvMap = new float2(1, 1);
            if (collision) {
                verticeList = verticeListFixedCol0;
                triangleList = triangleListFixedCol0;
                normalList = normalListFixedCol0;
            } else {
                verticeList = verticeListFixed0;
                triangleList = triangleListFixed0;
                normalList = normalListFixed0;
            }
        } else if (cubeIndex == 1) {
            uvMap = new float2(2, 3);
            uvList = uvCoordinates1;
            if (collision) {
                verticeList = verticeListFixedCol1;
                triangleList = triangleListFixedCol1;
                normalList = normalListFixedCol1;
            } else {
                verticeList = verticeListFixed1;
                triangleList = triangleListFixed1;
                normalList = normalListFixed1;
            }
        } else if (cubeIndex == 2) {
            uvMap = new float2(2, 0);
            uvList = uvCoordinates2;
            if (collision) {
                verticeList = verticeListFixedCol2;
                triangleList = triangleListFixedCol2;
                normalList = normalListFixedCol2;
            } else {
                verticeList = verticeListFixed2;
                triangleList = triangleListFixed2;
                normalList = normalListFixed2;
            }
        } else if (cubeIndex == 3) {
            uvMap = new float2(2, 1);
            uvList = uvCoordinates3;
            if (collision) {
                verticeList = verticeListFixedCol3;
                triangleList = triangleListFixedCol3;
                normalList = normalListFixedCol3;
            } else {
                verticeList = verticeListFixed3;
                triangleList = triangleListFixed3;
                normalList = normalListFixed3;
            }
        } else if (cubeIndex == 4) {
            uvMap = new float2(2, 2);
            uvList = uvCoordinates4;
            if (collision) {
                verticeList = verticeListFixedCol4;
                triangleList = triangleListFixedCol4;
                normalList = normalListFixedCol4;
            } else {
                verticeList = verticeListFixed4;
                triangleList = triangleListFixed4;
                normalList = normalListFixed4;
            }
        } else {
            uvMap = new float2(3, 1);
            uvList = uvCoordinates5;
            if (collision) {
                verticeList = verticeListFixedCol5;
                triangleList = triangleListFixedCol5;
                normalList = normalListFixedCol5;
            } else {
                verticeList = verticeListFixed5;
                triangleList = triangleListFixed5;
                normalList = normalListFixed5;
            }
        }

        int listCount;
        QuadTreeNodeJob[] list; //list of leaf nodes converted for to be compatible with jobs format
        if (collision) {
            list = cube[cubeIndex].quadtree.leafnodeJobsCollision;
            listCount = cube[cubeIndex].quadtree.collisionLeafNodeCount;
        } else {
            list = cube[cubeIndex].quadtree.leafnodeJobs;
            listCount = cube[cubeIndex].quadtree.leafNodeCount;
        }

        //convert to jobs format
        leafnodes = new NativeArray<QuadTreeNodeJob>(list, Allocator.TempJob);
        NativeArray<int> tmpTriangleN = new NativeArray<int>(tmpTriangle, Allocator.TempJob);
        NativeArray<int> tmpTriangleBorderedN = new NativeArray<int>(tmpTriangleBordered, Allocator.TempJob);
        
        //create job 
        CalculatePositionJob job = new CalculatePositionJob
        {
            nodesJob = leafnodes,
            verticesJob = verticeList,
            trianglesJob = triangleList,
            normalsJob = normalList,
            uvJob = uvList,
            radiusJob = radius,
            tmpTriangleJob = tmpTriangleN,
            tmpTriangleJobBordered = tmpTriangleBorderedN,
            borderedSizeIndex = borderedSizeIndex,

            useHeightmap = (terrainGenerationMode == TerrainGenerationMode.HEIGHTMAP_TEXTURE ? true : false),
            isCollision = collision,
            uvMap = uvMap,
            heightmapDimensions =heightmapDimensions,
            heightMap=planetTextureData,
            resJob = res,
            heightMapPower =heightMapPower
        };

        JobHandle jobHandle = job.Schedule(listCount, listCount / 12);
        jobHandle.Complete();

        tmpTriangleN.Dispose();
        tmpTriangleBorderedN.Dispose();
        borderedSizeIndex.Dispose();
        leafnodes.Dispose();
    }

    private void CalculateTriangle() {
        int count = 0;
        int tris = 0;
        tmpTriangleBordered = new int[(res + 1) * (res + 1) * 6];
        for (int i = 0; i < res + 2; i++) {
            for (int j = 0; j < res + 2; j++) {
                if (i != res + 1 && j != res + 1) {
                    tmpTriangleBordered[tris] = count; //0
                    tmpTriangleBordered[tris + 1] = count + 1; //1
                    tmpTriangleBordered[tris + 2] = count + res + 2; //2
                    tmpTriangleBordered[tris + 3] = count + 1; //1
                    tmpTriangleBordered[tris + 4] = count + res + 2 + 1; //2
                    tmpTriangleBordered[tris + 5] = count + res + 2;//3
                    tris += 6;
                }
                count += 1;
            }
        }

        tmpTriangle = new int[(res - 1) * (res - 1) * 6];
        count = 0;
        tris = 0;
        for (int i = 0; i < res; i++) {
            for (int j = 0; j < res; j++) {
                if (i != res - 1 && j != res - 1) {
                    {
                        tmpTriangle[tris] = count; //0
                        tmpTriangle[tris + 1] = count + 1; //1
                        tmpTriangle[tris + 2] = count + res; //2
                        tmpTriangle[tris + 3] = count + 1; //1
                        tmpTriangle[tris + 4] = count + res + 1; //2
                        tmpTriangle[tris + 5] = count + res;//3
                        tris += 6;
                    }
                }
                count += 1;
            }
        }
    }

    void UpdateRange() {
        if (range == null) {
            return;
        }
        
        for (int i = 0; i < range.Length; i++) {
            lod.detailLevelDist[i] = range[i];
        }
    }

    void InitMesh() {
        lod.MaxDetail = maxDetail;
        LOD.cameraPos = player;

        UpdateRange();

        if (cubeMesh == null) {
            cubeMesh = new Mesh[6];
            collisionMesh = new Mesh[6];
            for (int i = 0; i < 6; i++) {
                cubeMesh[i] = new Mesh();
                collisionMesh[i] = new Mesh();
            }
        }
        
        if (cube == null) {
            CreateCube();
        }

        if (meshFilters == null || meshFilters.Length == 0) {
            meshFilters = new MeshFilter[6];
            meshColliders = new MeshCollider[6];
            CreateCube();

            for (int i = 0; i < 6; i++) {
                GameObject meshObj = new GameObject("mesh" + i);

                meshObj.transform.parent = transform;
                meshObj.layer = gameObject.layer;
                meshObj.transform.localPosition = Vector3.zero;
                meshObj.transform.localRotation = quaternion.identity;
                meshObj.AddComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshColliders[i] = meshObj.AddComponent<MeshCollider>();
                meshFilters[i].sharedMesh = cube[i].mesh;
                meshColliders[i].sharedMesh = cube[i].collisionMesh;
                meshColliders[i].convex = false;
                meshObj.GetComponent<MeshRenderer>().material = Material;
                cube[i].meshCollider = meshColliders[i];

                meshChildren.Add(meshObj);
            };
        }
    }

    void CreateCube() {
        cube = new Plane[6];
        cube[0] = new Plane(cubeMesh[0], collisionMesh[0], Vector3.up      , res, this);
        cube[1] = new Plane(cubeMesh[1], collisionMesh[1], Vector3.left    , res, this);
        cube[2] = new Plane(cubeMesh[2], collisionMesh[2], Vector3.forward , res, this);
        cube[3] = new Plane(cubeMesh[3], collisionMesh[3], Vector3.right   , res, this);
        cube[4] = new Plane(cubeMesh[4], collisionMesh[4], Vector3.back    , res, this);
        cube[5] = new Plane(cubeMesh[5], collisionMesh[5], Vector3.down    , res, this);

        cube[0].SetNeighbors(cube[3], cube[1], cube[4], cube[2], DIRECTION.WEST, DIRECTION.WEST, DIRECTION.SOUTH, DIRECTION.NORTH);
        cube[1].SetNeighbors(cube[4], cube[2], cube[5], cube[0], DIRECTION.EAST, DIRECTION.EAST, DIRECTION.NORTH, DIRECTION.SOUTH);
        cube[2].SetNeighbors(cube[0], cube[5], cube[1], cube[3], DIRECTION.WEST, DIRECTION.WEST, DIRECTION.SOUTH, DIRECTION.NORTH);
        cube[3].SetNeighbors(cube[2], cube[4], cube[5], cube[0], DIRECTION.WEST, DIRECTION.WEST, DIRECTION.SOUTH, DIRECTION.NORTH);
        cube[4].SetNeighbors(cube[5], cube[0], cube[1], cube[3], DIRECTION.EAST, DIRECTION.EAST, DIRECTION.NORTH, DIRECTION.SOUTH);
        cube[5].SetNeighbors(cube[1], cube[3], cube[4], cube[2], DIRECTION.EAST, DIRECTION.EAST, DIRECTION.NORTH, DIRECTION.SOUTH);
    }
}

public static class DIRECTION 
{
    public static int EAST = 0;
    public static int NORTH = 1;
    public static int WEST = 2;
    public static int SOUTH = 3;
    public static int[] MIRROR_AXIS_X = new int[4] { 3, 2, 1, 0 };
    public static int[] MIRROR_AXIS_Y = new int[4] { 1, 0, 3, 2 };

    public static int[,,] DIRECTION_MAP = new int[4, 4, 4] {
        { { -1, 2, 1, -1 }, { -1, 1, 0, -1 }, { -1,  0,  3, -1 }, { -1, 3,2, -1 } },//E
        { { 2, 1, -1, -1 }, { 1, 0 , -1,-1 }, {  0,  3, -1, -1 }, {  3, 2,-1, -1 } },  //N
        { { 1, -1, -1, 2 }, { 0, -1, -1, 1 }, { 3, -1, -1, 0 }, { 2, -1, -1, 3} }, //W
        { { -1,-1 , 2, 1 }, { -1,-1,  1, 0}, { -1 ,-1 , 0,3 }, { -1,-1,  3, 2 } } //S
    };
}

public class LOD 
{
    public static Transform cameraPos;
    public int MaxDetail = 16;
    public Dictionary<int, float> detailLevelDist = new Dictionary<int, float>() {
        {0, Mathf.Infinity },
        {1, 60f  },
        {2, 25f  },
        {3, 10f  },
        {4, 4f   },
        {5, 1.5f },
        {6, 0.7f },
        {7, 0.3f },
        {8, 0.1f }
    };
}