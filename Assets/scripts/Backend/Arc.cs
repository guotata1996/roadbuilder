using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///   <para>t_start is in range (-PI, PI], 
///     sign(t_end - t_start) is determined by clockdir</para>
/// </summary>
public class Arc : Curve
{
    public Vector2 Center
    {
        get;
        private set;
    }

    public float Radius
    {
        get;
        private set;
    }

    public static Curve TryInit(Vector2 _center, Vector2 start, float angle)
    {
        if (Algebra.isclose(angle, 0f) || Mathf.Abs(angle) >= Mathf.PI * 2)
        {
            return null;
        }
        return new Arc(_center, start, angle);
    }

    /// <summary>
    ///   <para> start turns angle clockwise around center and arrives at end</para>
    /// </summary>
    /// <param name="angle">Can be negative</param>
    public static Curve TryInit(Vector2 start, float angle, Vector2 end)
    {
        if (Algebra.isclose(angle, 0f) || Mathf.Abs(angle) >= Mathf.PI * 2)
        {
            return null;
        }
        return new Arc(start, angle, end);
    }

    Arc(Vector2 _center, Vector2 start, float angle)
    {
        Center = _center;
        Radius = (start - _center).magnitude;
        var radialDir = start - _center;
        t_start = Mathf.Atan2(radialDir.y, radialDir.x);
        t_end = t_start + angle;
    }

    Arc(Vector2 start, float angle, Vector2 end)
    {
        var bottom_angle = (Mathf.PI - angle) * 0.5f;
        var Center = start + Algebra.RotatedY(end - start, -bottom_angle) * 0.5f / Mathf.Sin(angle / 2);
        Radius = (Center - start).magnitude;
        var radialDir = start - Center;
        t_start = Mathf.Atan2(radialDir.y, radialDir.x);
        t_end = t_start + angle;
    }

    Arc() { }

    protected override Vector2 _GetFrontDir(float t)
    {
        t = toGlobalParam(t);
        Vector2 radian_dir = new Vector2(Mathf.Cos(t), Mathf.Sin(t));
        return t_end > t_start ? Algebra.RotatedY(radian_dir, Mathf.PI / 2) : Algebra.RotatedY(radian_dir, -Mathf.PI / 2);
    }

    protected override float _GetLength()
    {
        return Mathf.Abs(t_end - t_start) * Radius;
    }

    protected override Vector2 _GetTwodPos(float t)
    {
        t = toGlobalParam(t);
        return Center + Radius * new Vector2(Mathf.Cos(t), Mathf.Sin(t));
    }

    protected override float? _ParamOf(Vector2 p)
    {
        Vector2 normalizedWorldDir = (p - Center) / Radius;
        if (!Algebra.isclose(normalizedWorldDir.magnitude, 1.0f))
        {
            return null;
        }
        float worldAngle = Mathf.Atan2(normalizedWorldDir.y, normalizedWorldDir.x);
        /// Since end_t falls in (-3PI, 3PI], 
        /// we should test every possible worldAngle in (-3PI, 3PI], 
        /// i.e. expand (-PI, PI] by 2 PI to both side
        float[] candidates = {worldAngle - Mathf.PI * 2, worldAngle, worldAngle + Mathf.PI * 2};
        foreach(var candidate in candidates)
        {
            if (Algebra.approximatelySmaller((candidate - t_end) * (candidate - t_start), 0f))
            {
                return Algebra.approximateTo01(toLocalParam(candidate), Length);
            }
        }
        return null;
    }

    protected override float _ToParamt(float unscaled_t)
    {
        return unscaled_t;
    }

    protected override float _ToUnscaledt(float t)
    {
        return t;
    }

    public override Curve DeepCopy()
    {
        Arc copy = new Arc();
        copy.Center = Center;
        copy.Radius = Radius;
        copy.t_start = t_start;
        copy.t_end = t_end;
        return copy;
    }

    public override string ToString()
    {
        return "Arc centered at " + Center + " with t_start = " + t_start + " ,t_end = " + t_end;
    }
}
