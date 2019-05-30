using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
///   <para>t_start is in range (-PI, PI], 
///     sign(t_end - t_start) is determined by clockdir</para>
/// </summary>
public class Arc : Curve
{
    public Vector2 Center
    {
        get
        {
            return controlPoints[1];
        }
        private set
        {
            controlPoints[1] = value;
        }
    }
    
    public Vector2 Start
    {
        get
        {
            return controlPoints[0];
        }
        private set
        {
            controlPoints[0] = value;
        }
    }

    public Vector2 End
    {
        get
        {
            return controlPoints[2];
        }
        private set
        {
            controlPoints[2] = value;
        }
    }

    // Once initialized, cannot change
    public float Radius
    {
        get;
        private set;
    }

    public override bool IsValid
    {
        get
        {
            return !float.IsInfinity(Center.x) && !float.IsInfinity(Start.x)
                && !float.IsInfinity(End.x) && !Algebra.isclose(Start, End);
        }
    }

    public static Curve GetDefault()
    {
        var rtn = new Arc();
        rtn.Center = rtn.Start = rtn.End = Vector2.negativeInfinity;
        return rtn;
    }

    public Arc(Vector2 _center, Vector2 start, float angle)
    {
        Debug.Assert(Mathf.Abs(angle) < Mathf.PI * 2);
        controlPoints = new Vector2[3];
        Center = _center;
        Radius = (start - _center).magnitude;
        var radialDir = start - _center;
        t_start = Mathf.Atan2(radialDir.y, radialDir.x);
        t_end = t_start + angle;
        Start = GetTwodPos(0f);
        End = GetTwodPos(1f);
    }

    Arc()
    {
        controlPoints = new Vector2[3];
    }

    /// <summary>
    ///   <para> start turns angle clockwise around center and arrives at end</para>
    /// </summary>
    /// <param name="angle">Can be negative</param>
    public Arc(Vector2 start, float angle, Vector2 end)
    {
        Debug.Assert(Mathf.Abs(angle) < Mathf.PI * 2);
        controlPoints = new Vector2[3];
        var bottom_angle = (Mathf.PI - angle) * 0.5f;
        Center = start + Algebra.RotatedY(end - start, -bottom_angle) * 0.5f / Mathf.Sin(angle / 2);
        Radius = (Center - start).magnitude;
        var radialDir = start - Center;
        t_start = Mathf.Atan2(radialDir.y, radialDir.x);
        t_end = t_start + angle;
        Start = GetTwodPos(0f);
        End = GetTwodPos(1f);
    }

    public override List<Vector2> ControlPoints
    {
        get
        {
            return controlPoints.ToList();
        }
        set
        {
            if (value[1] != Center && !float.IsInfinity(Start.x) && !float.IsInfinity(End.x))
            {
                // Only move center: keep endings. Projects control point to vertical split line.
                Center = Algebra.ProjectOn(value[1], (Start + End) / 2, 
                Algebra.RotatedY((End - Start).normalized, Mathf.PI / 2));

                var startRadialDir = Start - Center;
                var endRadialDir = End - Center;
                Radius = startRadialDir.magnitude;
                float new_t_start = Mathf.Atan2(startRadialDir.y, startRadialDir.x);
                float new_t_end = Mathf.Atan2(endRadialDir.y, endRadialDir.x);
                // preserve clockwise property

                if (t_start < t_end && new_t_start < new_t_end
                || t_start > t_end && new_t_start > new_t_end)
                {
                    // already preserved
                }
                else
                {
                    if (t_start < t_end && new_t_start > new_t_end)
                    {
                        new_t_end = new_t_end + 2 * Mathf.PI;
                    }
                    else
                    {
                        new_t_end = new_t_end - 2 * Mathf.PI;
                    }
                }

                t_start = new_t_start;
                t_end = new_t_end;
                NotifyShapeChanged();
            }
            else
            if (Start != value[0] || End != value[2])
            {
                // move ending, make default (angle=90). Notify only when all valid
                Start = value[0];
                End = value[2];
                if (!float.IsInfinity(Start.x) && !float.IsInfinity(End.x))
                {
                    Center = (Start + End) / 2 + Algebra.RotatedY((End - Start) / 2, - Mathf.PI / 2);
                    var startRadialDir = Start - Center;
                    Radius = startRadialDir.magnitude;
                    t_start = Mathf.Atan2(startRadialDir.y, startRadialDir.x);
                    t_end = t_start - Mathf.PI / 2;
                    NotifyShapeChanged();
                }
            }
             

        }
    }

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

    public override float GetMaximumCurvature => 1f / Radius;

    public override Vector2 GetAttractedPoint(Vector2 p, float attract_radius)
    {
        if ((p - Center).magnitude > attract_radius + Radius)
        {
            return p;
        }
        if (Algebra.isclose(Center, p) && attract_radius <= Radius)
        {
            return Center;
        }

        // ray (center -> p) intersects with arc
        Vector2 projected_p = Center + (p - Center).normalized * Radius;
        Vector2 closest_p;
        if (Contains(projected_p))
        {
            closest_p = projected_p;
        }
        else
        {
            closest_p = (p - Start).sqrMagnitude < (p - End).sqrMagnitude ?
                Start : End;
        }

        if ((closest_p - p).sqrMagnitude <= attract_radius * attract_radius)
        {
            return closest_p;
        }
        else
        {
            return p;
        }

    }

    public override Curve Clone()
    {
        Arc copy = new Arc();
        copy.Center = Center;
        copy.Radius = Radius;
        copy.t_start = t_start;
        copy.t_end = t_end;
        copy.NotifyShapeChanged();
        return copy;
    }

    public override void ShiftRight(float distance)
    {
        if (t_end > t_start)
        {
            Radius += distance;
        }
        else
        {
            Radius -= distance;
        }
        NotifyShapeChanged();
    }

    protected override void NotifyShapeChanged()
    {
        Start = GetTwodPos(0f);
        End = GetTwodPos(1f);
        base.NotifyShapeChanged();
    }

    public override string ToString()
    {
        return "Arc centered at " + Center + " Start = " + Start + " ,End =  " + End;
    }
}
