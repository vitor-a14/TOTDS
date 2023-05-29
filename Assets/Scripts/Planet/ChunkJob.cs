using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public struct ChunkJob : IJob
{
    public NativeArray<Vector3> vertices;
    public NativeArray<Vector3> borderVertices;
    public NativeArray<int> triangles;
    public NativeArray<int> borderTriangles;
    public NativeArray<Vector3> normals;

    public int quadIndex;
    public float radius;
    public float planetSize;
    public Vector3 localUp;
    public Vector3 position;
    public int triangleOffset;
    public int borderTriangleOffset;

    public void Execute() {
        Matrix4x4 transformMatrix;
        Vector3 rotationMatrixAttrib = new Vector3(0, 0, 0);
        Vector3 scaleMatrixAttrib = new Vector3(radius, radius, 1);

        if (localUp == Vector3.forward)
            rotationMatrixAttrib = new Vector3(0, 0, 180);
        else if (localUp == Vector3.back)
            rotationMatrixAttrib = new Vector3(0, 180, 0);
        else if (localUp == Vector3.right)
            rotationMatrixAttrib = new Vector3(0, 90, 270);
        else if (localUp == Vector3.left)
            rotationMatrixAttrib = new Vector3(0, 270, 270);
        else if (localUp == Vector3.up)
            rotationMatrixAttrib = new Vector3(270, 0, 90);
        else if (localUp == Vector3.down)
            rotationMatrixAttrib = new Vector3(90, 0, 270);

        transformMatrix = Matrix4x4.TRS(position, Quaternion.Euler(rotationMatrixAttrib), scaleMatrixAttrib);

        for (int i = 0; i < vertices.Length; i++) {
            Vector3 pointOnCube = transformMatrix.MultiplyPoint(Presets.quadTemplateVertices[quadIndex][i]);
            Vector3 pointOnUnitSphere = pointOnCube.normalized;
            float elevation = NoiseFilter.CalculateNoise(pointOnUnitSphere);
            vertices[i] = pointOnUnitSphere * (1 + elevation) * planetSize;
        }

        for (int i = 0; i < borderVertices.Length; i++) {
            Vector3 pointOnCube = transformMatrix.MultiplyPoint(Presets.quadTemplateBorderVertices[quadIndex][i]);
            Vector3 pointOnUnitSphere = pointOnCube.normalized;
            float elevation = NoiseFilter.CalculateNoise(pointOnUnitSphere);
            borderVertices[i] = pointOnUnitSphere * (1 + elevation) * planetSize;
        }

        //try to see if it works
        triangles.CopyFrom(Presets.quadTemplateTriangles[quadIndex]);
        borderTriangles.CopyFrom(Presets.quadTemplateBorderTriangles[quadIndex]);

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

            if (edgefansIndices[vertexIndexA] == 0)
                normals[vertexIndexA] += triangleNormal;
            if (edgefansIndices[vertexIndexB] == 0)
                normals[vertexIndexB] += triangleNormal;
            if (edgefansIndices[vertexIndexC] == 0)
                normals[vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = borderTriangles.Length / 3;

        for (int i = 0; i < borderTriangleCount; i++) {
            int normalTriangleIndex = i * 3;
            vertexIndexA = borderTriangles[normalTriangleIndex];
            vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

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
    }

    private int[] GetTrianglesWithOffset(int triangleOffset) {
        int[] newTriangles = new int[triangles.Length];

        for (int i = 0; i < triangles.Length; i++) {
            newTriangles[i] = triangles[i] + triangleOffset;
        }

        return newTriangles;
    }

    private int[] GetBorderTrianglesWithOffset(int borderTriangleOffset, int triangleOffset) {
        int[] newBorderTriangles = new int[borderTriangles.Length];

        for (int i = 0; i < borderTriangles.Length; i++) {
            newBorderTriangles[i] = (borderTriangles[i] < 0) ? borderTriangles[i] - borderTriangleOffset : borderTriangles[i] + triangleOffset;
        }

        return newBorderTriangles;
    }

    private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC) {
        Vector3 pointA = (indexA < 0) ? borderVertices[-indexA - 1] : vertices[indexA];
        Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }
}
