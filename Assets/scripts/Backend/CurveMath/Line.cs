using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Line : Curve
{

    public Vector2 Start
    {
        get;
        private set;
    }

    public Vector2 End
    {
        get;
        private set;
    }

    public override Vector2 GetAttractedPoint(Vector2 p, float attract_radius)
    {
        // p's projection on line
        Vector2 normalized_direction = (End - Start).normalized;
        Vector2 projected_p = Start + Vector2.Dot(p - Start, normalized_direction) * normalized_direction;
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

        if ((p - closest_p).sqrMagnitude <= attract_radius * attract_radius)
        {
            return closest_p;
        }
        else
        {
            return p;
        }
    }

    public override bool IsValid => !float.IsInfinity(Start.x) && !float.IsInfinity(End.x)
&& !Algebra.isclose(Start, End);

    protected override void Invalidate()
    {
        Start = End = Vector2.negativeInfinity;
    }

    public static Curve GetDefault()
    {
        return new Line(Vector2.negativeInfinity, Vector2.negativeInfinity);
    }

    public Line(Vector2 _Start, Vector2 _End)
    {
        Start = _Start;
        End = _End;
        t_start = 0f;
        t_end = 1f;
    }

    public override List<Vector2> ControlPoints
    {
        get
        {
            return new List<Vector2> { Start, End };
        }
        set
        {
            Start = value[0];
            End = value[1];

            if (IsValid)
            {
                NotifyShapeChanged();
            }
        }
    }

    protected override Vector2 _GetTwodPos(float unscaled_t)
    {
        return Vector2.Lerp(Start, End, unscaled_t);
    }

    protected override float _ToParamt(float unscaled_t)
    {
        return unscaled_t;
    }

    protected override float _ToUnscaledt(float t)
    {
        return t;
    }


    protected override Vector2 _GetFrontDir(float t)
    {
        return (End - Start).normalized;
    }

    protected override float? _ParamOf(Vector2 point)
    {
        float p;

        if (!Algebra.Parallel(point - Start, End - Start))
        {
            return null;
        }

        if (Algebra.isclose(End.x, Start.x))
        {
            p = (point.y - Start.y) / (End.y - Start.y);
        }
        else
        {
            p = (point.x - Start.x) / (End.x - Start.x);
        }
        return Algebra.approximateTo01(p, Length);
    }

    protected override float _GetLength()
    {
        return (Start - End).magnitude;
    }

    public override string ToString()
    {
        return "Line from " + Start + " to " + End;
    }

    public override float GetMaximumCurvature => 1 / Algebra.InfLength;

    /*<<<<<<<<<<<<<<<<<<<<<<<<<<<<>>>>>>>>>>>>>>>>>>>>>>>>>>>*/
    /*Extension Methods */

    public override void Crop(float unscaled_t_start, float unscaled_t_end)
    {
        var new_start = Vector2.Lerp(Start, End, unscaled_t_start);
        var new_end = Vector2.Lerp(Start, End, unscaled_t_end);
        Start = new_start;
        End = new_end;
        NotifyShapeChanged();
    }

    public override Curve Clone()
    {
        return new Line(Start, End);
    }

    public override void ShiftRight(float distance)
    {
        var newStart = Start + GetRightDir(0f) * distance;
        var newEnd = End + GetRightDir(1f) * distance;
        Start = newStart;
        End = newEnd;

        NotifyShapeChanged();
    }
}
