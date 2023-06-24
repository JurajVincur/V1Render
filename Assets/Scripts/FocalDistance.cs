using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FocalDistance : MonoBehaviour
{
    public float focalDistance;
    public Camera eyeCamera;
    public Transform ellipsoid;
    public int res = 4;
    public float pupilRadius = 0.002f;
    public bool flat = false;
    public bool drawRays = true;
    public Transform planeToFit;
    public bool tweenDistance = false;
    public Vector2 tweenMinMax;

    private List<Vector3> focalPoints = new List<Vector3>();
    private Vector3 position = Vector3.zero;
    private Vector3 normal = Vector3.zero;
    private float tweenDirection = 1f;

    void OnDrawGizmos()
    {
        Transform eyeTransform = eyeCamera.transform;
        float fds = focalDistance * focalDistance;
        Gizmos.DrawWireSphere(eyeTransform.position, focalDistance);
        Ray[] rays = new Ray[2];

        focalPoints.Clear();

        for (int i = flat ? res / 2 : 0; i < (flat ? res / 2 + 1 : res); i++)
        {
            float v = i / (res - 1.0f);
            for (int j = 0; j < res; j++)
            {
                float u = j / (res - 1.0f);
                Ray worldRay = eyeCamera.ViewportPointToRay(new Vector2(u, v));
                Vector3 localOrigin = eyeTransform.InverseTransformPoint(worldRay.origin);
                Vector3 localDir = eyeTransform.InverseTransformVector(worldRay.direction);
                Vector3 sphereIntersection = RayTraceUtils.RayToSphereIntersection(localDir, localOrigin, eyeTransform.position, fds);
                //Debug.DrawLine(eyeTransform.position, eyeTransform.TransformPoint(sphereIntersection));
                for (int k = -1; k <= 1; k += 2)
                {
                    var o = eyeTransform.position + new Vector3(k * pupilRadius, 0f, 0f);
                    var d = Vector3.Normalize(eyeTransform.TransformPoint(sphereIntersection) - o);
                    if (drawRays)
                    {
                        Debug.DrawRay(o, d * focalDistance, Color.red);
                    }
                    worldRay = new Ray(o, d);
                    localOrigin = ellipsoid.InverseTransformPoint(worldRay.origin);
                    localDir = ellipsoid.InverseTransformVector(worldRay.direction);
                    Vector3 sphereIntersection2 = RayTraceUtils.RayToSphereIntersection(localDir, localOrigin, Vector3.zero);
                    Vector3 sphereNormal = RayTraceUtils.IntersectionToNormal(sphereIntersection2);
                    Vector3 ellipsoidNormal = sphereNormal;
                    ellipsoidNormal.x /= Mathf.Pow(ellipsoid.localScale.x / 2.0f, 2.0f);
                    ellipsoidNormal.y /= Mathf.Pow(ellipsoid.localScale.y / 2.0f, 2.0f);
                    ellipsoidNormal.z /= Mathf.Pow(ellipsoid.localScale.z / 2.0f, 2.0f);
                    Vector3 worldEllipsoidNormal = Vector3.Normalize(ellipsoid.TransformVector(ellipsoidNormal));
                    float factor = -2.0f * Vector3.Dot(worldEllipsoidNormal, worldRay.direction);
                    Vector3 worldReflected = Vector3.Normalize(worldEllipsoidNormal * factor + worldRay.direction);
                    Vector3 worldIntersection = ellipsoid.TransformPoint(sphereIntersection2);
                    //Debug.DrawRay(worldRay.origin, worldRay.direction, Color.white);
                    //Debug.DrawLine(worldIntersection, worldReflected, Color.red);
                    rays[(k + 1) / 2] = new Ray(worldIntersection, Vector3.Normalize(worldReflected - worldIntersection));
                }
                Vector3 intersection = RayTraceUtils.RayRayIntersection(rays[0], rays[1]);
                Gizmos.DrawSphere(intersection, 0.001f);
                if (drawRays)
                {
                    Debug.DrawLine(rays[0].origin, intersection, Color.red);
                    Debug.DrawLine(rays[1].origin, intersection, Color.red);
                }
                focalPoints.Add(intersection);
            }
        }
        if (planeToFit != null)
        {
            Fit.Plane(focalPoints, out position, out normal);
            planeToFit.position = position;
            planeToFit.forward = normal;
        }
    }

    private void Update()
    {
        float speed = 1f;
        if (tweenDistance)
        {
            focalDistance += Time.deltaTime * speed * tweenDirection;
            if (focalDistance > tweenMinMax.y)
            {
                focalDistance = tweenMinMax.y;
                tweenDirection = -1;
            }
            else if (focalDistance < tweenMinMax.x)
            {
                focalDistance = tweenMinMax.x;
                tweenDirection = 1;
            }
        }
    }
}
