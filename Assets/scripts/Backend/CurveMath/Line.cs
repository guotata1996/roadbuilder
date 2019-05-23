using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Line : Curve
{

    public Vector2 Start
    {
        get => controlPoints[0];
        private set => controlPoints[0] = value;
    }

    public Vector2 End
    {
        get => controlPoints[1];
        private set => controlPoints[1] = value;
    }

    public override bool IsValid => !float.IsInfinity(Start.x) && !float.IsInfinity(End.x) 
    && !Algebra.isclose(Start, End);
    
    public static Curve GetDefault()
    {
        return new Line(Vector2.negativeInfinity, Vector2.negativeInfinity);
    }

    public Line(Vector2 _Start, Vector2 _End)
    {
        controlPoints = new Vector2[2];
        Start = _Start;
        End = _End;
        t_start = 0f;
        t_end = 1f;
    }

    public override List<Vector2> ControlPoints
    {
        get
        {
            return controlPoints.ToList();
        }
        set
        {
            if (Algebra.isclose(value[0], value[1]))
            {
                Debug.LogWarning("Line ctrl points too close");
                return;
            }
            Start = value[0];
            End = value[1];

            if (!float.IsInfinity(Start.x) && !float.IsInfinity(End.x))
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
        _CommitChanges();
    }

    public override Curve DeepCopy()
    {
        return new Line(Start, End);
    }
}
