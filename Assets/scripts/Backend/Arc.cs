using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Arc : Curve
{
    /* Radius only calculated by center and start
     * If start==end, represents a circle
     */
    public float curveslope = 0.1f;

    public Vector2 center;
    public float radius;
    /* t_start is in range[-Pi, Pi]
     * t_end < 2 * PI + t_start*/

    /*If angle>0, 
     *angle is in radius 
     */
    public Arc(Vector2 _center, Vector2 start, float angle, float _z_start = 0f, float _z_end = 0f)
    {
        center = _center;
        radius = (start - _center).magnitude;
        float t_0 = Mathf.Acos((start.x - _center.x) / radius); /*[0, Pi]*/
        if (!Algebra.isclose(Mathf.Sin(t_0), (start.y - center.y) / radius))
        {
            t_0 = -t_0;/*[-Pi, 0]*/
        }
        Debug.Assert(Mathf.Abs(t_0) <= Mathf.PI);

        t_start = t_0;
        t_end = t_start + angle;

        z_start = _z_start;
        z_offset = _z_end - _z_start;

        Debug.Assert(!Algebra.isclose(this.length, 0f));
    }

    public Arc(Vector3 center3, Vector3 start3, float angle)
    {

        Vector2 _center = Algebra.toVector2(center3);
        Vector2 start = Algebra.toVector2(start3);

        center = _center;
        radius = (start - _center).magnitude;
        float t_0 = Mathf.Acos((start.x - _center.x) / radius); /*[0, Pi]*/
        if (!Algebra.isclose(Mathf.Sin(t_0), (start.y - center.y) / radius))
        {
            t_0 = -t_0;/*[-Pi, 0]*/
        }
        Debug.Assert(Mathf.Abs(t_0) <= Mathf.PI);
        t_start = t_0;
        t_end = t_start + angle;

        z_start = center3.y;
        z_offset = 0f;

        Debug.Assert(!Algebra.isclose(this.length, 0f));

    }

    protected Arc() { }

    /*start-end: clockwise*/
    public Arc(Vector2 _start, float angle, Vector2 _end, float _z_start = 0f, float _z_end = 0f)
    {
        center = (_start + _end) / 2f + new Vector2((_end - _start).y, -(_end - _start).x).normalized * 0.5f * (_start - _end).magnitude / Mathf.Tan(angle / 2);
        radius = 0.5f * (_start - _end).magnitude / Mathf.Sin(angle / 2);
        t_start = new Line(center, _start, 0f, 0f).angle_ending(true);
        t_end = new Line(center, _end, 0f, 0f).angle_ending(true);
        if (t_start < t_end)
        {
            t_end -= Mathf.PI * 2;
        }
        Debug.Assert(!counterClockwise);
        z_start = _z_start;
        z_offset = _z_end - _z_start;

        Debug.Assert(!Algebra.isclose(this.length, 0f));
    }

    private Arc deepCopy()
    {
        Arc copy = new Arc();
        copy.center = this.center;
        copy.radius = this.radius;
        copy.z_start = this.z_start;
        copy.z_offset = this.z_offset;
        copy.t_start = this.t_start;
        copy.t_end = this.t_end;
        return copy;
    }

    public override Vector3 at(float t)
    {
        float parametric_t = toGlobalParam(t);
        float _x = center.x + radius * Mathf.Cos(parametric_t);
        float _z = center.y + radius * Mathf.Sin(parametric_t);
        float _y = z_start + z_offset * t;
        return new Vector3(_x, _y, _z);
    }

    public override Vector2 at_2d(float t)
    {
        float parametric_t = toGlobalParam(t);
        float _x = center.x + radius * Mathf.Cos(parametric_t);
        float _y = center.y + radius * Mathf.Sin(parametric_t);
        return new Vector2(_x, _y);
    }

    public override Vector3 upNormal(float t)
    {
        float parametric_t = toGlobalParam(t);
        float tanGradient = z_offset / this.length;
        return new Vector3(Mathf.Sin(parametric_t) * tanGradient, 1, -Mathf.Cos(parametric_t) * tanGradient).normalized;
    }

    public override Vector3 frontNormal(float t)
    {
        float parametric_t = toGlobalParam(t);
        float tanGradient = z_offset / this.length;

        return counterClockwise ? -new Vector3(Mathf.Sin(parametric_t), tanGradient, -Mathf.Cos(parametric_t)).normalized :
               new Vector3(Mathf.Sin(parametric_t), tanGradient, -Mathf.Cos(parametric_t)).normalized;
    }

    public override float length
    {
        get
        {
            return Mathf.Abs(t_end - t_start) * radius;
        }
    }

    bool counterClockwise
    {
        get
        {
            return t_end > t_start;
        }
    }

    public override float maximumCurvature
    {
        get
        {
            return 1f / radius;
        }
    }

    public override float angle_2d(float t)
    {
        float ans_candidate;
        t = toGlobalParam(t);

        ans_candidate = counterClockwise ? t + Mathf.PI / 2 : t - Mathf.PI / 2;

        while (ans_candidate < 0)
        {
            ans_candidate += Mathf.PI * 2;
        }
        while (ans_candidate >= 2 * Mathf.PI)
        {
            ans_candidate -= Mathf.PI * 2;
        }
        Debug.Assert(0 <= ans_candidate && ans_candidate < 2 * Mathf.PI);
        if (ans_candidate < 0 || ans_candidate >= 2 * Mathf.PI)
        {
            Debug.Log(t + "'s ans = " + ans_candidate + "(tstart=) " + t_start + " (tend=) " + t_end);
        }
        return ans_candidate;
    }

    public override Vector3 rightNormal(float t)
    {
        float parametric_t = toGlobalParam(t);
        if (t_end > t_start)
            return new Vector3(Mathf.Cos(parametric_t), 0f, Mathf.Sin(parametric_t));
        else
            return new Vector3(-Mathf.Cos(parametric_t), 0f, -Mathf.Sin(parametric_t));
    }

    public override List<Curve> segmentation(float maxlen)
    {
        int segCount = Mathf.CeilToInt(this.length / maxlen);

        List<Curve> segments = new List<Curve>();
        for (int i = 0; i != segCount; ++i)
        {
            Arc a_seg = this.deepCopy();
            float start_frac = i * maxlen / this.length;
            float end_frac = Mathf.Min((i + 1) * maxlen / this.length, 1f);
            float temp_tstart = t_start + start_frac * (t_end - t_start);
            float temp_tend = t_start + end_frac * (t_end - t_start);
            float temp_zstart = z_start + start_frac * z_offset;
            float temp_zoffset = (end_frac - start_frac) * z_offset;
            a_seg.t_start = temp_tstart;
            a_seg.t_end = temp_tend;
            a_seg.z_start = temp_zstart;
            a_seg.z_offset = temp_zoffset;
            segments.Add(a_seg);
        }
        return segments;
    }

    public override float TravelAlong(float currentParam, float distToTravel, bool zeroToOne)
    {
        if (zeroToOne){
            return Mathf.Min(1f, currentParam + distToTravel / length);
        }
        else{
            return Mathf.Max(0f, currentParam - distToTravel / length);
        }
    }

    public override float? paramOf(Vector2 point)
    {

        if (!Algebra.isclose((point - center).magnitude, radius))
        {
            return null;
        }

        float angle = Mathf.Acos((point.x - center.x) / radius); //[0,PI]
        float sinvalue = (point.y - center.y) / radius;
        if (!Algebra.isclose(Mathf.Sin(angle), sinvalue))
        {
            angle = -angle; //[-PI, PI]
        }
        while (angle > Mathf.Max(t_start, t_end) && !Algebra.isclose(angle, Mathf.Max(t_start, t_end)))
            angle -= 2 * Mathf.PI;

        while (angle < Mathf.Min(t_start, t_end) && !Algebra.isclose(angle, Mathf.Min(t_start, t_end)))
            angle += 2 * Mathf.PI;
        float p = (angle - t_start) / (t_end - t_start);
        return Algebra.approximateTo01(p, length);
    }

    public override string ToString()
    {
        return string.Format("Arc: t_start={0:C3} point={1:C3}, t_end={2:C3} point={3:C3}, z_start={4} z_offset={5}",
                             t_start, at_ending_2d(true), t_end, at_ending_2d(false), z_start, z_offset);
    }

    public override Vector3 AttouchPoint(Vector3 p)
    {
        float angle;
        if (p.x == center.x)
        {
            angle = Mathf.PI / 2;
        }
        else
        {
            angle = Mathf.Atan((center.y - p.z) / (center.x - p.x));
        }

        List<float> candidateAngle = new List<float>() { angle };

        candidateAngle.Add(angle + Mathf.PI);

        candidateAngle.Add(angle - Mathf.PI);

        var validAngle = candidateAngle.Where(a => (a - t_start) * (a - t_end) < 0).ToList();
        validAngle.Add(t_start);
        validAngle.Add(t_end);
        var sortedValid = validAngle.OrderBy(a => Mathf.Pow(center.x + Mathf.Cos(a) * radius - p.x, 2) + Mathf.Pow(center.y + Mathf.Sin(a) * radius - p.z, 2));
        float ans = sortedValid.First();
        float localparam = toLocalParam(ans);
        //return new Vector2(center.x + Mathf.Cos(ans) * radius, center.y + Mathf.Sin(ans) * radius);
        return this.at(localparam);
    }

    public override Curve concat(Curve b)
    {
        Debug.Assert(b is Arc);
        if (Algebra.isclose(t_start, b.t_start))
        {
            return new Arc(center, at_ending_2d(false), (t_start - t_end) + (b.t_end - b.t_start), z_start + z_offset, b.z_offset - z_offset);
        }
        if (Algebra.isclose(t_start, b.t_end))
        {
            return new Arc(center, at_ending_2d(false), (t_start - t_end) + (b.t_start - b.t_end), z_start + z_offset, -b.z_offset - z_offset);
        }
        if (Algebra.isclose(t_end, b.t_start))
        {
            return new Arc(center, at_ending_2d(true), (t_end - t_start) + (b.t_end - b.t_start), z_start, z_offset + b.z_offset);
        }
        if (Algebra.isclose(t_end, b.t_end))
        {
            return new Arc(center, at_ending_2d(true), (t_end - t_start) + (b.t_start - b.t_end), z_start, z_offset - b.z_offset);
        }
        Debug.Assert(false);
        return null;
    }

}