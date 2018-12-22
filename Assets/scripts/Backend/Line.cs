using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : Curve
{
    public Vector2 start, end;

    public Line(Vector2 _start, Vector2 _end, float _z_start = 0f, float _z_end = 0f)
    {
        start = _start;
        end = _end;
        z_start = _z_start;
        z_offset = _z_end - _z_start;
        t_start = 0f;
        t_end = 1f;

        Debug.Assert(!Algebra.isclose(this.length, 0f));
    }

    public Line(Vector3 _start, Vector3 _end)
    {
        start = Algebra.toVector2(_start);
        end = Algebra.toVector2(_end);
        z_start = _start.y;
        z_offset = _end.y - _start.y;
        t_start = 0f;
        t_end = 1f;

        Debug.Assert(!Algebra.isclose(this.length, 0f));
    }

    public override Vector3 at(float t)
    {
        t = toGlobalParam(t);
        float _x = start.x * (1 - t) + end.x * t;
        float _y = z_start + z_offset * t;
        float _z = start.y * (1 - t) + end.y * t;
        return new Vector3(_x, _y, _z);
    }

    public override Vector2 at_2d(float t)
    {
        t = toGlobalParam(t);
        return start * (1 - t) + end * t;
    }

    public override float length
    {
        get
        {
            return (start - end).magnitude;
        }
    }

    public override float maximumCurvature
    {
        get
        {
            return 1 / Algebra.InfLength;
        }
    }

    public override float angle_2d(float t)
    {
        if (Algebra.isclose(at_2d(0f).x, at_2d(1f).x))
        {
            if (at_2d(1f).y > at_2d(0f).y)
                return Mathf.PI / 2;
            else
                return Mathf.PI * 1.5f;
        }
        else
        {
            float candidate = Mathf.Atan((at_2d(0f).y - at_2d(1f).y) / (at_2d(0f).x - at_2d(1f).x));
            if (at_2d(0f).x > at_2d(1f).x)
            {
                candidate += Mathf.PI;
            }
            else
            {
                if (candidate < 0)
                    candidate += Mathf.PI * 2;
            }
            Debug.Assert(0 <= candidate && candidate < Mathf.PI * 2);
            return candidate;
        }
    }

    public override Vector3 upNormal(float t)
    {
        Vector2 tangentdir = (at_2d(1f) - at_2d(0f)).normalized;
        float tanGradient = z_offset / this.length;
        return new Vector3(tangentdir.x * tanGradient, 1f, tangentdir.y * tanGradient);
    }

    public override Vector3 frontNormal(float t){
        Vector2 tangentdir = (at_2d(1f) - at_2d(0f)).normalized;
        float tanGradient = z_offset / this.length;
        return new Vector3(tangentdir.x, tanGradient, tangentdir.y);
    }

    public override Vector3 rightNormal(float t)
    {
        Vector2 tangentdir = (at_2d(1f) - at_2d(0f)).normalized;
        return new Vector3(tangentdir.y, 0f, -tangentdir.x);
    }

    public override List<Curve> segmentation(float maxlen)
    {
        Debug.Assert(maxlen != 0f);
        int segCount = Mathf.CeilToInt(this.length / maxlen);
        List<Curve> result = new List<Curve>();
        for (int i = 0; i != segCount; ++i)
        {
            float start_frac = i * maxlen / this.length;
            float end_frac = Mathf.Min((i + 1) * maxlen / this.length, 1f);
            start_frac = toGlobalParam(start_frac);
            end_frac = toGlobalParam(end_frac);
            if (!Algebra.isclose(start_frac * (end - start) + start, end_frac * (end - start) + start))
            {
                Line l2 = new Line(start_frac * (end - start) + start, end_frac * (end - start) + start, z_start + start_frac * z_offset, z_start + end_frac * z_offset);
                result.Add(l2);
            }
        }
        return result;
    }

    public override float TravelAlong(float currentParam, float distToTravel, bool zeroToOne){
        if (zeroToOne){
            return Mathf.Min(1f, currentParam + distToTravel / length);
        }
        else{
            return Mathf.Max(0f, currentParam - distToTravel / length);
        }
    }


    public override float? paramOf(Vector2 point)
    {
        float p;

        if (!Geometry.Parallel(point - start, end - start))
        {
            return null;
        }

        if (Algebra.isclose(end.x, start.x))
        {

            p = (point.y - start.y) / (end.y - start.y);
        }
        else
        {
            p = (point.x - start.x) / (end.x - start.x);
        }
        return Algebra.approximateTo01(toLocalParam(p), length);

    }

    public override string ToString()
    {
        return string.Format("Line: Start = {0} ; End = {1}, zStart = {2}, zOffset = {3}", at_2d(0f), at_2d(1f), z_start, z_offset);
    }


    public override Vector3 AttouchPoint(Vector3 p)
    {
        Vector2 twod_p = new Vector2(p.x, p.z);
        float candidate_t = -((end.x - start.x) * (start.x - p.x) + (end.y - start.y) * (start.y - p.z))
            / (Mathf.Pow(end.x - start.x, 2) + Mathf.Pow(end.y - start.y, 2));
        candidate_t = toLocalParam(candidate_t);
        if (0 < candidate_t && candidate_t < 1)
        {
            return this.at(candidate_t);
        }
        else
        {
            if ((twod_p - this.at_2d(0)).magnitude < (twod_p - this.at_2d(1)).magnitude)
            {
                return this.at(0);
            }
            else
            {
                return this.at(1);
            }
        }
    }

    public override Curve concat(Curve b)
    {
        Debug.Assert(b is Line);
        if (Algebra.isclose(at_ending_2d(true), b.at_ending_2d(true)))
        {
            return new Line(at_ending_2d(false), b.at_ending_2d(false), at(1f).y, b.at(1f).y - at(1f).y);
        }
        if (Algebra.isclose(at_ending_2d(true), b.at_ending_2d(false)))
        {
            return new Line(at_ending_2d(false), b.at_ending_2d(true), at(0f).y, b.at(1f).y - at(0f).y);
        }
        if (Algebra.isclose(at_ending_2d(false), b.at_ending_2d(true)))
        {
            return new Line(at_ending_2d(true), b.at_ending_2d(false), at(1f).y, b.at(0f).y - at(1f).y);
        }
        if (Algebra.isclose(at_ending_2d(false), b.at_ending_2d(false)))
        {
            return new Line(at_ending_2d(true), b.at_ending_2d(true), at(0f).y, b.at(0f).y - at(0f).y);
        }
        Debug.Assert(false);
        return null;
    }
}
