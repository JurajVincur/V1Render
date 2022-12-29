using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTrace : MonoBehaviour
{
    public Transform ellipsoid;
    public Camera eyeCamera;
    public MeshFilter meshFilter;
    public int res = 4;

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
                Vector3 sphereIntersection = RayToSphereIntersection(localDir, localOrigin);
                Vector3 sphereNormal = IntersectionToNormal(sphereIntersection);
                Vector3 ellipsoidNormal = sphereNormal;
                ellipsoidNormal.x /= Mathf.Pow(ellipsoid.localScale.x / 2.0f, 2.0f);
                ellipsoidNormal.y /= Mathf.Pow(ellipsoid.localScale.y / 2.0f, 2.0f);
                ellipsoidNormal.z /= Mathf.Pow(ellipsoid.localScale.z / 2.0f, 2.0f);
                Vector3 worldEllipsoidNormal = Vector3.Normalize(ellipsoid.TransformVector(ellipsoidNormal));
                float factor = -2.0f * Vector3.Dot(worldEllipsoidNormal, worldRay.direction);
                Vector3 worldReflected = Vector3.Normalize(worldEllipsoidNormal * factor + worldRay.direction);
                Vector3 worldIntersection = ellipsoid.TransformPoint(sphereIntersection);
                //Debug.DrawRay(worldRay.origin, worldRay.direction, Color.white);
                Debug.DrawLine(worldRay.origin, worldIntersection, Color.red);
                Debug.DrawRay(worldIntersection, worldEllipsoidNormal * 0.01f, Color.yellow);
                //Debug.DrawRay(worldIntersection, worldReflected * 0.2f, Color.blue);

                Vector3 worldPlaneIntersection = RayToPlaneIntersections(worldReflected, worldIntersection);
                Debug.DrawLine(worldIntersection, worldPlaneIntersection, Color.blue);
                vertices.Add(meshFilter.transform.InverseTransformPoint(worldPlaneIntersection));
            }
        }

        meshFilter.mesh.SetVertices(vertices);
        meshFilter.sharedMesh.UploadMeshData(false);
    }

    //see https://diegoinacio.github.io/computer-vision-notebooks-page/pages/ray-intersection_sphere.html
    Vector3 RayToSphereIntersection(Vector3 direction, Vector3 origin) //ellipsoid local dir and local origin
    {
        float rsq = 0.25f;
        Vector3 sphereCenter = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 ndir = Vector3.Normalize(direction);
        Vector3 orientedSegment = sphereCenter - origin;
        float t = Vector3.Dot(orientedSegment, ndir);

        Vector3 pe = origin + ndir * t;
        Vector3 tmp = pe - sphereCenter;
        float dsq = Vector3.Dot(tmp, tmp);
        float i = Mathf.Sqrt(rsq - dsq);

        Vector3 ps = origin + ndir * (t + i);
        return ps;
    }

    Vector3 IntersectionToNormal(Vector3 intersection)
    {
        Vector3 sphereCenter = new Vector3(0.0f, 0.0f, 0.0f);
        var normal = intersection - sphereCenter;
        return Vector3.Normalize(normal);
    }

    Vector3 RayToPlaneIntersections(Vector3 direction, Vector3 origin)
    {
        Transform pt = meshFilter.transform;
        Vector3 normal = pt.forward;
        float denom = Vector3.Dot(normal, direction);
        if (Mathf.Abs(denom) > 1e-6)
        {
            Vector3 segment = pt.position - origin;
            float t = Vector3.Dot(segment, normal) / denom;
            if (t > 0)
            {
                Vector3 intersection = origin + direction * t;
                return intersection;
            }
        }
        return Vector3.zero;
    }
}
