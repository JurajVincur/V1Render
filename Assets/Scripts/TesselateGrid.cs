using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TesselateGrid : MonoBehaviour
{
    public MeshFilter meshFilter;
    public int distortionMeshRes = 32;

    private void Awake()
    {
        var mesh = meshFilter.mesh;
        mesh.name = "Tesellated Grid";

        //Generate the Vertices
        int xSize = distortionMeshRes, ySize = distortionMeshRes;
        var vertices = new List<Vector3>((xSize + 1) * (ySize + 1) * 2);
        var uvs = new List<Vector2>((xSize + 1) * (ySize + 1) * 2);
        for (int i = 0, y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                vertices.Add(new Vector2((float)x / xSize, (float)y / ySize) - (Vector2.one * 0.5f));
                uvs.Add(new Vector2((float)x / xSize, (float)y / ySize));
            }
        }
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);

        //Generate the Triangles
        int[] triangles = new int[(xSize * ySize * 6 * 2) +
                                 ((xSize + ySize) * 6 * 2)];
        for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
        {
            for (int x = 0; x < xSize; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
                triangles[ti + 5] = vi + xSize + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.UploadMeshData(false);
    }
}
