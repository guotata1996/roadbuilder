using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static Curve TryInit(Vector2 _Start, Vector2 _End)
    {
        if (Algebra.isclose(_Start, _End)){
            Debug.LogWarning("Try creating Line whose Length = 0");
            return null;
        }
        return new Line(_Start, _End);
    }

    Line(Vector2 _Start, Vector2 _End)
    {
        Start = _Start;
        End = _End;
        t_start = 0f;
        t_end = 1f;
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
