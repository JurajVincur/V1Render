using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTrace : MonoBehaviour
{
    public Transform ellipsoid;
    public Camera eyeCamera;
    public MeshFilter meshFilter;
    public int res = 4;
    public bool flat = false;
    public int flati = 0;

    void Update()
    {
        List<Vector3> vertices = new List<Vector3>();
        for (int i = 0; i < res; i++)
        {
            float v = i / (res - 1.0f);
            for (int j = 0; j < res; j++)
            {
                float u = j / (res - 1.0f);
                Ray worldRay = eyeCamera.ViewportPointToRay(new Vector2(u, v));
                Vector3 localOrigin = ellipsoid.InverseTransformPoint(worldRay.origin);
                Vector3 localDir = ellipsoid.InverseTransformVector(worldRay.direction);
                Vector3 sphereIntersection = RayTraceUtils.RayToSphereIntersection(localDir, localOrigin, Vector3.zero);
                Vector3 sphereNormal = RayTraceUtils.IntersectionToNormal(sphereIntersection);
                Vector3 ellipsoidNormal = sphereNormal;
                ellipsoidNormal.x /= Mathf.Pow(ellipsoid.localScale.x / 2.0f, 2.0f);
                ellipsoidNormal.y /= Mathf.Pow(ellipsoid.localScale.y / 2.0f, 2.0f);
                ellipsoidNormal.z /= Mathf.Pow(ellipsoid.localScale.z / 2.0f, 2.0f);
                Vector3 worldEllipsoidNormal = Vector3.Normalize(ellipsoid.TransformVector(ellipsoidNormal));
                float factor = -2.0f * Vector3.Dot(worldEllipsoidNormal, worldRay.direction);
                Vector3 worldReflected = Vector3.Normalize(worldEllipsoidNormal * factor + worldRay.direction);
                Vector3 worldIntersection = ellipsoid.TransformPoint(sphereIntersection);
                Vector3 worldPlaneIntersection = RayTraceUtils.RayToPlaneIntersections(worldReflected, worldIntersection, meshFilter.transform);

                if (flat == false || i == flati)
                {
                    Debug.DrawRay(worldRay.origin, worldRay.direction * 0.1f, Color.yellow);
                    Debug.DrawLine(worldRay.origin, worldIntersection, Color.green);
                    //Debug.DrawRay(worldIntersection, worldEllipsoidNormal * 0.01f, Color.yellow);
                    //Debug.DrawRay(worldIntersection, worldReflected * 0.2f, Color.blue);
                    Debug.DrawLine(worldIntersection, worldPlaneIntersection, Color.green);
                }

                vertices.Add(meshFilter.transform.InverseTransformPoint(worldPlaneIntersection));
            }
        }

        meshFilter.mesh.SetVertices(vertices);
        meshFilter.sharedMesh.UploadMeshData(false);
    }
}
