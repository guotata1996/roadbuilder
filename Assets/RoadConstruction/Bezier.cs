using UnityEngine;

public class Bezier
{
    Vector3 p0, p1, p2;
    bool isStraight;

    private Bezier(Vector3 _p0, Vector3 _p1, Vector3 _p2) {
        p0 = _p0;
        p1 = _p1;
        p2 = _p2;
        isStraight = false;
    }

    private Bezier(Vector3 _from, Vector3 _to)
    {
        p0 = _from;
        p1 = _to;
        isStraight = true;
    }

    public static Bezier Create(Ray from, Ray to)
    {
        Vector3 middle;
        Ray reversedTo = new Ray(to.origin, -to.direction);
        if (Math3d.RayRayIntersection(out middle, from, reversedTo))
        {
            return new Bezier(from.origin, middle, to.origin);
        }
        else
        {
            if (Vector3.Dot(from.direction, to.direction) > 0.999f && Vector3.Dot((to.origin - from.origin).normalized, from.direction) > 0.999f)
            {
                return new Bezier(from.origin, to.origin);
            }
            return null;
        }
    }

    public Vector3 GetPoint(float t)
    {
        if (isStraight)
        {
            return Math3d.GetPointOnSpline(t, new Vector3[] { p0, p1});
        }
        else
        {
            return Math3d.GetPointOnSpline(t, new Vector3[] { p0, p1, p2 });
        }
    }

    public Vector3 GetForward(float t)
    {
        Vector3 _p1 = GetPoint(t - 0.05f);
        Vector3 _p2 = GetPoint(t + 0.05f);
        return (_p2 - _p1).normalized;
    }

    public Vector3 GetRight(float t)
    {
        return Vector3.Cross(Vector3.up, GetForward(t)).normalized;
    }

}
