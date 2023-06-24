using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RayTraceUtils
{
    //see https://diegoinacio.github.io/computer-vision-notebooks-page/pages/ray-intersection_sphere.html
    public static Vector3 RayToSphereIntersection(Vector3 direction, Vector3 origin, Vector3 sphereCenter, float rsq = 0.25f) //ellipsoid local dir and local origin
    {
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

    public static Vector3 IntersectionToNormal(Vector3 intersection)
    {
        Vector3 sphereCenter = new Vector3(0.0f, 0.0f, 0.0f);
        var normal = intersection - sphereCenter;
        return Vector3.Normalize(normal);
    }

    public static Vector3 RayToPlaneIntersections(Vector3 direction, Vector3 origin, Transform pt)
    {
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

    public static float ClosestAlphaOnSegmentToLine(Vector3 segA, Vector3 segB, Vector3 lineA, Vector3 lineB)
    {
        Vector3 lineBA = lineB - lineA; float lineDirSqrMag = Vector3.Dot(lineBA, lineBA);
        Vector3 inPlaneA = segA - ((Vector3.Dot(segA - lineA, lineBA) / lineDirSqrMag) * lineBA),
                inPlaneB = segB - ((Vector3.Dot(segB - lineA, lineBA) / lineDirSqrMag) * lineBA);
        Vector3 inPlaneBA = inPlaneB - inPlaneA;
        return (lineDirSqrMag != 0f && inPlaneA != inPlaneB) ? Vector3.Dot(lineA - inPlaneA, inPlaneBA) / Vector3.Dot(inPlaneBA, inPlaneBA) : 0f;
    }

    public static Vector3 RayRayIntersection(Ray rayA, Ray rayB)
    {
        return Vector3.LerpUnclamped(rayA.origin, rayA.origin + rayA.direction,
         ClosestAlphaOnSegmentToLine(rayA.origin, rayA.origin + rayA.direction,
                                     rayB.origin, rayB.origin + rayB.direction));
    }
}
