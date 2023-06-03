using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

[BurstCompile]
public struct CalculatePositionJob : IJobParallelFor 
{
    [ReadOnly] public NativeArray<QuadTreeNodeJob> nodesJob;
    
    [NativeDisableParallelForRestriction] public NativeArray<float3> verticesJob;
    [NativeDisableParallelForRestriction] public NativeArray<float3> normalsJob;
    [NativeDisableParallelForRestriction] public NativeArray<int> trianglesJob;
    [NativeDisableParallelForRestriction] public NativeArray<float2> uvJob;

    [ReadOnly] public NativeArray<int> tmpTriangleJob;
    [ReadOnly] public NativeArray<int> tmpTriangleJobBordered;
    [ReadOnly] public NativeArray<int> borderedSizeIndex;
    [ReadOnly] public int resJob;
    [ReadOnly] public float radiusJob;
    [ReadOnly] public float baseFrequency;
    [ReadOnly] public float2 uvMap; 
    [ReadOnly] public bool isCollision;
    [ReadOnly] public NativeArray<float> heightMap;
    [ReadOnly] public int2 heightmapDimensions;
    [ReadOnly] public float heightMapPower;

    public void Execute(int index) {
        NativeArray<float3> tmpVertices = new NativeArray<float3>(resJob * resJob, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        NativeArray<float3> tmpNormals = new NativeArray<float3>(resJob * resJob, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        NativeArray<float2> tmpUV = new NativeArray<float2>(resJob * resJob, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        ConvertToVertices(nodesJob[index],tmpVertices,tmpNormals,tmpUV);

        for (int j = 0; j < tmpVertices.Length; j++) {
            verticesJob[j + nodesJob[index].verticeIndexStart] = tmpVertices[j];
        }
        
        for (int j = 0; j < tmpTriangleJob.Length; j++) {
            trianglesJob[j + nodesJob[index].triangleIndexStart] = tmpTriangleJob[j] + nodesJob[index].verticeIndexStart;
        }

        for (int j = 0; j < tmpNormals.Length; j++) {
            normalsJob[j + nodesJob[index].verticeIndexStart] = tmpNormals[j];
        }

        if (isCollision == false) {
            for (int j = 0; j < tmpUV.Length; j++) {
                uvJob[j + nodesJob[index].verticeIndexStart] = tmpUV[j];
            }
        }
        
        tmpVertices.Dispose();
        tmpNormals.Dispose();
        tmpUV.Dispose();
    }

    public void ConvertToVertices(QuadTreeNodeJob node,NativeArray<float3>   verticeArray, NativeArray<float3> normalArray, NativeArray<float2> uvArray) {
        NativeArray<float3> borderedVerticeArray = new NativeArray<float3>((resJob + 2) * (resJob + 2), Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        NativeArray<float3> normalsBorder = new NativeArray<float3>((resJob + 2) * (resJob + 2), Allocator.Temp, NativeArrayOptions.ClearMemory);
        NativeArray<float2> uvBorder = new NativeArray<float2>((resJob + 2) * (resJob + 2), Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        int count = 0;
        float3 pointOnCube;
        float3 pointOnSphere;

        for (int i = 0; i < resJob + 2; i++) {
            for (int j = 0; j < resJob + 2; j++) {
                float2 percent = new float2(j - 1, i - 1) / (resJob - 1);
                pointOnCube = node.center + ((percent.x - 0.5f) * 2 * node.axisA+ (percent.y - 0.5f) * 2 * node.axisB) * node.radius;

                //calculate uvs 
                float2 Coordinate2D = new float2();  
                if (node.localUp.x != 0) {
                        Coordinate2D.x = pointOnCube.z;
                        Coordinate2D.y = pointOnCube.y;
                } else if (node.localUp.y != 0) {
                    if (node.localUp.y > 0) {
                        Coordinate2D.x = pointOnCube.z;
                        Coordinate2D.y = pointOnCube.x;
                    } else {
                        Coordinate2D.x = pointOnCube.z;
                        Coordinate2D.y = pointOnCube.x;
                    }
                } else {
                    if (node.localUp.z > 0) {
                        Coordinate2D.x = pointOnCube.x;
                        Coordinate2D.y = pointOnCube.y;
                    } else {
                        Coordinate2D.x = pointOnCube.x;
                        Coordinate2D.y = pointOnCube.y;
                    }
                }

                Coordinate2D.x += radiusJob;
                Coordinate2D.y += radiusJob;
                Coordinate2D  /= (radiusJob * 2);

                if (node.localUp.x != 0) { 
                    if (node.localUp.x > 0) {
                        Coordinate2D.x = 1 - Coordinate2D.x;
                        Coordinate2D.y = 1 - Coordinate2D.y;
                    } else { 
                        Coordinate2D.y = 1 - Coordinate2D.y;
                    }   
                } else if (node.localUp.y != 0) {
                    if (node.localUp.y > 0) {
                        Coordinate2D.x = 1 - Coordinate2D.x;

                    } else {
                        Coordinate2D.x = 1 - Coordinate2D.x;
                        Coordinate2D.y = 1 - Coordinate2D.y;
                    }
                } else {
                    if (node.localUp.z < 0) {
                        Coordinate2D.x = 1 - Coordinate2D.x;
                        Coordinate2D.y = 1 - Coordinate2D.y;
                    } else {  
                        Coordinate2D.y = 1 - Coordinate2D.y;
                    }
                }

                Coordinate2D.x = (Coordinate2D.x / 4) + uvMap.y*0.25f;
                Coordinate2D.y = (Coordinate2D.y / 4) + uvMap.x*0.25f;
                uvBorder[count] = Coordinate2D;
                pointOnSphere = math.normalize(pointOnCube);
                borderedVerticeArray[count] = pointOnSphere * (radiusJob+heightMapPower*CalculateHeightMap(uvBorder[count]));
                count += 1;
            }
        }

        count = 0;
        for (int i = 0; i < resJob + 2; i++) {
            for (int j = 0; j < resJob + 2; j++) {
                //north
                if (node.neighbours1) {
                    if (j == resJob && i % 2 == 0 && i > 0 && i < resJob + 1) {
                        borderedVerticeArray[count] = (borderedVerticeArray[count - (resJob + 2)] + borderedVerticeArray[count + (resJob + 2)]) / 2; 
                    }
                }
                //south
                if (node.neighbours3) {
                    if (j == 1 && i % 2 == 0 && i > 0 && i < resJob + 1) {
                        borderedVerticeArray[count] = (borderedVerticeArray[count - (resJob + 2)] + borderedVerticeArray[count + (resJob + 2)]) / 2; 
                    }
                }
                //east
                if (node.neighbours0) {
                    if (i == resJob && j % 2 == 0) {

                        borderedVerticeArray[count] = (borderedVerticeArray[count - 1] + borderedVerticeArray[count + 1]) / 2;
                    }
                }
                //west
                if (node.neighbours2) {
                    if (i == 1 && j % 2 == 0) {
                        borderedVerticeArray[count] = (borderedVerticeArray[count - 1] + borderedVerticeArray[count + 1]) / 2;
                    }
                }
                count += 1;
            }
        }
        
        count = 0;
        if (node.edgeNeighbors0 || node.edgeNeighbors1 || node.edgeNeighbors2 || node.edgeNeighbors3) {
            for (int i = 0; i < resJob + 2; i++) {
                for (int j = 0; j < resJob + 2; j++) {
                    //north
                    if (node.edgeNeighbors1) {
                        if (j == resJob && i % 2 == 0 && i > 0 && i < resJob + 1) {
                            borderedVerticeArray[count] = (borderedVerticeArray[count - (resJob + 2)] + borderedVerticeArray[count + (resJob + 2)]) / 2;
                        }
                    }
                    //south
                    if (node.edgeNeighbors3) {
                        if (j == 1 && i % 2 == 0 && i > 0 && i < resJob + 1) {
                            borderedVerticeArray[count] = (borderedVerticeArray[count - (resJob + 2)] + borderedVerticeArray[count + (resJob + 2)]) / 2;
                        }
                    }
                    //east
                    if (node.edgeNeighbors0) {
                        if (i == resJob && j % 2 == 0) {
                            borderedVerticeArray[count] = (borderedVerticeArray[count - 1] + borderedVerticeArray[count + 1]) / 2;
                        }
                    }
                    //west
                    if (node.edgeNeighbors2) {
                        if (i == 1 && j % 2 == 0) {
                            borderedVerticeArray[count] = (borderedVerticeArray[count - 1] + borderedVerticeArray[count + 1]) / 2;
                        }
                    }
                    count += 1;
                }
            }
        }

        CalculateNormals(borderedVerticeArray, tmpTriangleJobBordered, normalsBorder);

        int c = 0;
        for (int i = 0; i < borderedVerticeArray.Length; i++) {
            if (borderedSizeIndex[i] >= 0) {
                verticeArray[borderedSizeIndex[i]] = (borderedVerticeArray[i]);
                normalArray[borderedSizeIndex[i]] = normalsBorder[i];
                uvArray[borderedSizeIndex[i]] = uvBorder[i];
                c += 1;
            }
        }

        borderedVerticeArray.Dispose();
        normalsBorder.Dispose();
        uvBorder.Dispose();
    }

    float CalculateHeightMap(float2 uv) {
        return BilinearFiltering(math.clamp(uv.x, 0, 1), math.clamp(uv.y, 0, 1));
    }

    float BilinearFiltering(float xf,float yf) {
        int w = heightmapDimensions.x-1;
        int h = heightmapDimensions.y-1;
        int x1 =(int) math.floor(xf * w);
        int y1 =(int) math.floor(yf * h);
        int x2 =(int)math.clamp(x1 + 1, 0, w);
        int y2 =(int)math.clamp(y1 + 1, 0, h);

        float xp = xf * w - x1;
        float yp = yf * h - y1;

        float p11 = GetPixel(x1, y1);
        float p21 = GetPixel(x2, y1);
        float p12 = GetPixel(x1, y2);
        float p22 = GetPixel(x2, y2);

        float px1 = math.lerp( p11, p21,xp);
        float px2 = math.lerp( p12, p22,xp);

        return math.lerp( px1, px2,yp);
    }
    
    float GetPixel(int x,int y) {
        return heightMap[(x + heightmapDimensions.x * y)];
    }

    void CalculateNormals(NativeArray<float3> vertices, NativeArray<int> triangles, NativeArray<float3> normals) {
        int triangleCount = triangles.Length / 3;
        int normalTriangleIndex;
        int vertexIndexA;
        int vertexIndexB;
        int vertexIndexC;
        float3 pointA;
        float3 pointB;
        float3 pointC;

        float3 sideAB;
        float3 sideAC;
        
        float3 triangleNormal;

        for (int i = 0; i < triangleCount; i++) {
            normalTriangleIndex = i * 3;
            vertexIndexA = triangles[normalTriangleIndex];
            vertexIndexB = triangles[normalTriangleIndex + 1];
            vertexIndexC = triangles[normalTriangleIndex + 2];
            
            pointA = vertices[vertexIndexA];
            pointB = vertices[vertexIndexB];
            pointC = vertices[vertexIndexC];

            sideAB = pointB - pointA;
            sideAC = pointC - pointA;
            
            triangleNormal = math.cross(sideAB, sideAC);
            triangleNormal = math.normalize(triangleNormal);
            
            normals[vertexIndexA] += triangleNormal;
            normals[vertexIndexB] += triangleNormal;
            normals[vertexIndexC] += triangleNormal;
        }

        int len = normals.Length;
        for (int i = 0; i < len; i++) {
            normals[i] = math.normalize(normals[i]);
        }
    }
}

public struct QuadTreeNodeJob {
    public float3 center;
    public float radius;
    public bool neighbours0;
    public bool neighbours1;
    public bool neighbours2;
    public bool neighbours3;
    public int detailLevel;
    public float3 localUp;
    public float3 axisA;
    public float3 axisB;
    public int verticeIndexStart;
    public int triangleIndexStart;
    public int edgeDirection1;
    public int edgeDirection2;
    public bool edgeNeighbors0;
    public bool edgeNeighbors1;
    public bool edgeNeighbors2;
    public bool edgeNeighbors3;
}
