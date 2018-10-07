using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public abstract class Curve
{
    public float z_start, z_offset;
    public float t_start, t_end;
    /*
	 * Normal of the road surface
	 */
    public abstract Vector3 upNormal(float t);

    public abstract Vector3 rightNormal(float t);

    public abstract Vector3 at(float t);

    public abstract Vector2 at_2d(float t);

    public abstract float length { get; }

    /* range [0, 2Pi)
     */
    public float angle_ending(bool start, float offset = 0f)
    {
        offset = offset / length;
        if (start)
            return angle_2d(offset);
        else
        {
            float raw_angle = angle_2d(1f - offset);
            return raw_angle >= Mathf.PI ? raw_angle - Mathf.PI : raw_angle + Mathf.PI;
        }
    }

    public Vector2 at_ending(bool start, float offset = 0f)
    {
        offset = offset / length;
        if (start)
            return at_2d(offset);
        else
        {
            return at_2d(1f - offset);
        }

    }

    public abstract float angle_2d(float t);

    public Vector2 direction_ending_2d(bool start)
    {
        float angle = angle_ending(start);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    public Vector2 direction_2d(float t)
    {
        return new Vector2(Mathf.Cos(angle_2d(t)), Mathf.Sin(angle_2d(t)));
    }

    /* Separate the original curve to pieces <= maxLen
	 * eg. original length = 18, maxlen = 5
	 * res:[5,5,5,3]
	 */
    public abstract List<Curve> segmentation(float maxlen, bool keep_length = true);

    /* split curve into two parts: 1:0~t 2:t~1
     */

    private void reverse()
    {
        float tmp = t_start;
        t_start = t_end;
        t_end = tmp;
        z_start += z_offset;
        z_offset = -z_offset;
    }

    protected float toGlobalParam(float t)
    {
        return t_start + t * (t_end - t_start);
    }

    protected float toLocalParam(float global_t)
    {
        return (global_t - t_start) / (t_end - t_start);
    }

    public List<Curve> split(float cutpoint)
    {
        if (cutpoint >= 0.5f)
        {
            return segmentation(this.length * cutpoint, keep_length: false);
        }
        else
        {
            this.reverse();
            List<Curve> reversedSegment = segmentation(this.length * (1 - cutpoint), keep_length: false);
            foreach (Curve seg in reversedSegment)
            {
                seg.reverse();
            }
            reversedSegment.Reverse();
            this.reverse();
            return reversedSegment;
        }
    }


    /*TODO: test special case: 0/1 */
    public Curve cut(float start, float end)
    {
        Debug.Assert(0 <= start && start < end && end <= 1);
        Curve start_to_1 = split(start).Last();
        float secondFraction = (end - start) / (1f - start);
        return start_to_1.split(secondFraction).First();
    }

    public abstract float? paramOf(Vector2 point);

    public bool contains_2d(Vector2 point)
    {
        return (paramOf(point) != null && 0 <= (float)paramOf(point) && (float)paramOf(point) <= 1f);
    }

    public abstract override string ToString();

    public abstract Vector2 AttouchPoint(Vector2 p);

    public abstract Curve concat(Curve another);

}

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

    /*If angle>0, clockwise
     *angle is in radius 
     */
    public Arc(Vector2 _center, Vector2 start, float angle, float _z_start, float _z_offset)
    {
        Debug.Assert(angle < 2 * Mathf.PI);
        center = _center;
        radius = (start - _center).magnitude;
        float t_0 = Mathf.Acos((start.x - _center.x) / radius); /*[0, Pi]*/
        if (!Mathf.Approximately(Mathf.Sin(t_0), (start.y - center.y) / radius))
        {
            t_0 = -t_0;/*[-Pi, 0]*/
        }
        t_start = t_0;
        t_end = t_start + angle;

        z_start = _z_start;
        z_offset = _z_offset;
    }

    /*start-end: clockwise*/
    public Arc(Vector2 _start, float angle, Vector2 _end, float _z_start, float _z_end)
    {
        center = (_start + _end) / 2f + new Vector2((_end - _start).y, -(_end - _start).x).normalized * 0.5f * (_start - _end).magnitude / Mathf.Tan(angle / 2);
        radius = 0.5f * (_start - _end).magnitude / Mathf.Sin(angle / 2);
        t_start = new Line(center, _start, 0f, 0f).angle_ending(true);
        t_end = new Line(center, _end, 0f, 0f).angle_ending(true);
        if (t_end - t_start > Mathf.PI)
        {
            t_end -= Mathf.PI * 2;
        }
        else
        {
            if (t_start - t_end > Mathf.PI)
            {
                t_start -= Mathf.PI;
            }
        }

        z_start = _z_start;
        z_offset = _z_end;
    }

    private Arc()
    {
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
        float parametric_t = t_start + (t_end - t_start) * t;
        float _x = center.x + radius * Mathf.Cos(parametric_t);
        float _z = center.y + radius * Mathf.Sin(parametric_t);
        float _y = z_start + z_offset * t;
        return new Vector3(_x, _y, _z);
    }

    public override Vector2 at_2d(float t)
    {
        float parametric_t = t_start + (t_end - t_start) * t;
        float _x = center.x + radius * Mathf.Cos(parametric_t);
        float _y = center.y + radius * Mathf.Sin(parametric_t);
        return new Vector2(_x, _y);
    }

    public override Vector3 upNormal(float t)
    {
        float parametric_t = t_start + (t_end - t_start) * t;
        float tanGradient = z_offset / this.length;
        return new Vector3(Mathf.Sin(parametric_t) * tanGradient, 1, -Mathf.Cos(parametric_t) * tanGradient).normalized;
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
            return Mathf.Sin(t_end - t_start) > 0;
        }
    }

    public override float angle_2d(float t)
    {
        float ans_candidate;
        t = toGlobalParam(t);

        ans_candidate = counterClockwise ? t + Mathf.PI / 2 : t - Mathf.PI / 2;

        if (ans_candidate < 0)
        {
            ans_candidate += Mathf.PI * 2;
        }
        if (ans_candidate >= 2 * Mathf.PI)
            ans_candidate -= Mathf.PI * 2;
        Debug.Assert(0 <= ans_candidate && ans_candidate < 2 * Mathf.PI);
        return ans_candidate;
    }

    public override Vector3 rightNormal(float t)
    {
        float parametric_t = t_start + (t_end - t_start) * t;
        if (t_end > t_start)
            return new Vector3(Mathf.Cos(parametric_t), 0f, Mathf.Sin(parametric_t));
        else
            return new Vector3(-Mathf.Cos(parametric_t), 0f, -Mathf.Sin(parametric_t));
    }

    public override List<Curve> segmentation(float maxlen, bool keep_length = true)
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
        while (angle > Mathf.Max(t_start, t_end))
            angle -= 2 * Mathf.PI;

        while (angle < Mathf.Min(t_start, t_end))
            angle += 2 * Mathf.PI;

        float p = (angle - t_start) / (t_end - t_start);

        return Algebra.approximateTo01(p);
    }

    public override string ToString()
    {
        return string.Format("Arc: Length={0}, t_start={1}, t_end={2}", length, t_start, t_end);
    }

    public override Vector2 AttouchPoint(Vector2 p)
    {
        float angle;
        if (p.x == center.x)
        {
            angle = Mathf.PI / 2;
        }
        else
        {
            angle = Mathf.Atan((center.y - p.y) / (center.x - p.x));
        }

        List<float> candidateAngle = new List<float>() { angle };

        candidateAngle.Add(angle + Mathf.PI);

        candidateAngle.Add(angle - Mathf.PI);

        var validAngle = candidateAngle.Where(a => (a - t_start) * (a - t_end) < 0).ToList();
        validAngle.Add(t_start);
        validAngle.Add(t_end);
        var sortedValid = validAngle.OrderBy(a => Mathf.Pow(center.x + Mathf.Cos(a) * radius - p.x, 2) + Mathf.Pow(center.y + Mathf.Sin(a) * radius - p.y, 2));
        float ans = sortedValid.First();
        return new Vector2(center.x + Mathf.Cos(ans) * radius, center.y + Mathf.Sin(ans) * radius);
    }

    public override Curve concat(Curve b)
    {
        Debug.Assert(b is Arc);
        if (Algebra.isclose(t_start, b.t_start))
        {
            return new Arc(center, at_ending(false), (t_start - t_end) + (b.t_end - b.t_start), z_start + z_offset, b.z_offset - z_offset);
        }
        if (Algebra.isclose(t_start, b.t_end))
        {
            return new Arc(center, at_ending(false), (t_start - t_end) + (b.t_start - b.t_end), z_start + z_offset, -b.z_offset - z_offset);
        }
        if (Algebra.isclose(t_end, b.t_start))
        {
            return new Arc(center, at_ending(true), (t_end - t_start) + (b.t_end - b.t_start), z_start, z_offset + b.z_offset);
        }
        if (Algebra.isclose(t_end, b.t_end))
        {
            return new Arc(center, at_ending(true), (t_end - t_start) + (b.t_start - b.t_end), z_start, z_offset - b.z_offset);
        }
        Debug.Assert(false);
        return null;
    }

}

public class Line : Curve
{
    public Vector2 start, end;

    public Line(Vector2 _start, Vector2 _end, float _z_start, float _z_offset)
    {
        start = _start;
        end = _end;
        z_start = _z_start;
        z_offset = _z_offset;
        t_start = 0f;
        t_end = 1f;
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

    public override Vector3 rightNormal(float t)
    {
        Vector2 tangentdir = (at_2d(1f) - at_2d(0f)).normalized;
        return new Vector3(tangentdir.y, 0f, -tangentdir.x);
    }

    public override List<Curve> segmentation(float maxlen, bool keep_length = true)
    {
        int segCount = Mathf.CeilToInt(this.length / maxlen);
        List<Curve> result = new List<Curve>();
        for (int i = 0; i != segCount; ++i)
        {
            float start_frac = i * maxlen / this.length;
            float end_frac = Mathf.Min((i + 1) * maxlen / this.length, 1f);
            start_frac = toGlobalParam(start_frac);
            end_frac = toGlobalParam(end_frac);
            Line l2 = new Line(start_frac * (end - start) + start, end_frac * (end - start) + start, z_start + start_frac * z_offset, z_offset * (end_frac - start_frac));
            result.Add(l2);
        }
        return result;
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
        return Algebra.approximateTo01(toLocalParam(p));

    }

    public override string ToString()
    {
        return string.Format("Line: Start = {0} ; End = {1}, t_start = {2}, t_end = {3}", start, end, t_start, t_end);
    }

    public override Vector2 AttouchPoint(Vector2 p)
    {
        float candidate_t = -((end.x - start.x) * (start.x - p.x) + (end.y - start.y) * (start.y - p.y))
            / (Mathf.Pow(end.x - start.x, 2) + Mathf.Pow(end.y - start.y, 2));
        candidate_t = toLocalParam(candidate_t);
        if (0 < candidate_t && candidate_t < 1)
        {
            return this.at_2d(candidate_t);
        }
        else
        {
            if ((p - this.at_2d(0)).magnitude < (p - this.at_2d(1)).magnitude)
            {
                return this.at_2d(0);
            }
            else
            {
                return this.at_2d(1);
            }
        }
    }

    public override Curve concat(Curve b)
    {
        Debug.Assert(b is Line);
        if (Algebra.isclose(at_ending(true), b.at_ending(true)))
        {
            return new Line(at_ending(false), b.at_ending(false), at(1f).y, b.at(1f).y - at(1f).y);
        }
        if (Algebra.isclose(at_ending(true), b.at_ending(false)))
        {
            return new Line(at_ending(false), b.at_ending(true), at(0f).y, b.at(1f).y - at(0f).y);
        }
        if (Algebra.isclose(at_ending(false), b.at_ending(true)))
        {
            return new Line(at_ending(true), b.at_ending(false), at(1f).y, b.at(0f).y - at(1f).y);
        }
        if (Algebra.isclose(at_ending(false), b.at_ending(false)))
        {
            return new Line(at_ending(true), b.at_ending(true), at(0f).y, b.at(0f).y - at(0f).y);
        }
        Debug.Assert(false);
        return null;
    }
}

public class Bezeir : Curve
{
    public Vector2 P0, P1, P2;
    public Bezeir(Vector2 _P0, Vector2 _P1, Vector2 _P2, float _z_start, float _z_offset)
    {
        Debug.Assert(!Geometry.Parallel(_P1 - _P0, _P2 - _P1));
        P0 = _P0;
        P1 = _P1;
        P2 = _P2;
        z_start = _z_start;
        z_offset = _z_offset;
        t_start = 0f;
        t_end = 1f;
    }

    public override Vector3 at(float t)
    {
        t = toGlobalParam(t);
        float _y = z_start + z_offset * t;
        Vector2 x_z = (1 - t) * (1 - t) * P0 + 2 * t * (1 - t) * P1 + t * t * P2;
        return new Vector3(x_z.x, _y, x_z.y);
    }

    public override Vector2 at_2d(float t)
    {
        t = toGlobalParam(t);
        return (1 - t) * (1 - t) * P0 + 2 * t * (1 - t) * P1 + t * t * P2;
    }

    public override float length
    {
        get
        {
            return lengthFromZeroTo(1f);
        }
    }

    public override float angle_2d(float t)
    {
        t = toGlobalParam(t);
        Vector2 tangentdir = (2 * (t - 1) * P0 + (2 - 4 * t) * P1 + 2 * t * P2);
        return new Line(Vector2.zero, tangentdir, 0f, 0f).angle_2d(t);
    }

    private float lengthIntegral(float t)
    {
        float A1 = 2 * P2.x - 4 * P1.x + 2 * P0.x;
        float A0 = -2 * P0.x + 2 * P1.x;
        float B1 = 2 * P2.y - 4 * P1.y + 2 * P0.y;
        float B0 = -2 * P0.y + 2 * P1.y;
        /*int(sqrt(Ax2+Bx+C))*/
        float A = A1 * A1 + B1 * B1;
        float B = 2 * A1 * A0 + 2 * B1 * B0;
        float C = A0 * A0 + B0 * B0;
        if (Algebra.isclose(A, 0f))
        {
            Vector3 param_t_coordinate = this.at(t);
            return (new Vector2(param_t_coordinate.x, param_t_coordinate.z) - P0).magnitude;
        }
        else
        {
            /*http://www.wolframalpha.com/input/?i=integral+sqrt(Ax%5E2%2BBx%2BC) */
            float firstItem = (2 * A * t + B) * Mathf.Sqrt(t * (A * t + B) + C) / (4 * A);
            float secondItem = (B * B - 4 * A * C) * Mathf.Log(2 * Mathf.Sqrt(A) * Mathf.Sqrt(t * (A * t + B) + C) + 2 * A * t + B) / (8 * Mathf.Pow(A, 1.5f));
            return firstItem - secondItem;
        }
    }

    private float lengthFromZeroTo(float t)
    {
        float temp_t_end = toGlobalParam(t);
        float temp_t_start = toGlobalParam(0);
        return Mathf.Abs(lengthIntegral(temp_t_end) - lengthIntegral(temp_t_start));
    }

    private float lengthGradient(float t)
    {
        t = toGlobalParam(t);
        Vector2 dxy_dt = 2 * (t - 1) * P0 + (2 - 4 * t) * P1 + 2 * t * P2;
        return dxy_dt.magnitude / Mathf.Abs(t_end - t_start);
    }

    public override Vector3 upNormal(float t)
    {
        t = t_start + (t_end - t_start) * t;
        Vector2 tangentdir = (2 * (t - 1) * P0 + (2 - 4 * t) * P1 + 2 * t * P2).normalized;
        float tanGradient = z_offset * (t_end - t_start) / this.length;
        return new Vector3(tangentdir.x * tanGradient, 1f, tangentdir.y * tanGradient);
    }

    public override Vector3 rightNormal(float t)
    {
        t = t_start + (t_end - t_start) * t;
        Vector2 tangentdir = (2 * (t - 1) * P0 + (2 - 4 * t) * P1 + 2 * t * P2).normalized;
        return new Vector3(tangentdir.y, 0f, -tangentdir.x);
    }

    public override List<Curve> segmentation(float maxlen, bool keep_length = true)
    {
        List<Curve> result = new List<Curve>();
        float lastEnd = 0;
        int fragCount = Mathf.CeilToInt(this.length / maxlen);
        for (int multipler = 0; multipler < fragCount; multipler++)
        {
            float thisEnd;
            if (keep_length)
                thisEnd = Algebra.newTown(this.lengthFromZeroTo, this.lengthGradient, Mathf.Min(this.length, (float)(multipler + 1) * maxlen), Mathf.Min(1f, maxlen / this.length * (multipler + 1)));
            else
            {
                thisEnd = Mathf.Min(1f, maxlen / this.length * (multipler + 1));
            }

            Bezeir fragment = new Bezeir(this.P0, this.P1, this.P2, this.z_start, this.z_offset);
            fragment.t_start = toGlobalParam(lastEnd);
            fragment.t_end = toGlobalParam(thisEnd);
            result.Add(fragment);
            lastEnd = thisEnd;
        }
        return result;
    }

    public override float? paramOf(Vector2 point)
    {
        float A11 = (P0.y - 2 * P1.y + P2.y) * 2 * (P1.x - P0.x);
        float A12 = (P0.x - 2 * P1.x + P2.x) * 2 * (P1.y - P0.y);
        float A1 = A11 - A12;

        float A01 = (P0.y - 2 * P1.y + P2.y) * (P0.x - point.x);
        float A02 = (P0.x - 2 * P1.x + P2.x) * (P0.y - point.y);
        float A0 = A01 - A02;
        float realparam = Algebra.functionSolver(A0, A1, 0f)[0];
        realparam = toLocalParam(realparam);
        if (!Algebra.isclose((this.at_2d(realparam) - point).magnitude, 0f))
        {
            return null;
        }
        else
            return Algebra.approximateTo01(realparam);
    }

    public override string ToString()
    {
        return string.Format("Beizier: Length={0}, t_start={1}, t_end={2}，P0 = {3}, P1 = {4}, P2 = {5}", length, t_start, t_end, P0, P1, P2);
    }

    public Vector2 targetP;

    public override Vector2 AttouchPoint(Vector2 p)
    {

        targetP = p;
        float t1 = Algebra.newTown(this.DeriveOfDistance, this.DeriveOfDeriveOfDistance, 0f, t_start);
        float t2 = Algebra.newTown(this.DeriveOfDistance, this.DeriveOfDeriveOfDistance, 0f, t_end);
        t1 = toLocalParam(t1);
        t2 = toLocalParam(t2);
        List<float> candidateParams = new List<float>();
        if (0 < t1 && t1 < 1)
            candidateParams.Add(t1);
        if (0 < t2 && t2 < 1)
            candidateParams.Add(t2);
        candidateParams.Add(0f);
        candidateParams.Add(1f);

        var sortedParams = candidateParams.OrderBy((param) => (this.at_2d(param) - p).magnitude);
        return this.at_2d(sortedParams.First());
    }

    public float[] DistanceParams()
    {
        float a2_x = P0.x - 2 * P1.x + P2.x;
        float a1_x = -2 * P0.x + 2 * P1.x;
        float a0_x = P0.x - targetP.x;

        float a2_y = P0.y - 2 * P1.y + P2.y;
        float a1_y = -2 * P0.y + 2 * P1.y;
        float a0_y = P0.y - targetP.y;

        float[] res = new float[5];
        res[4] = Mathf.Pow(a2_x, 2f) + Mathf.Pow(a2_y, 2f);
        res[3] = 2 * (a1_x * a2_x + a1_y * a2_y);
        res[2] = Mathf.Pow(a1_x, 2f) + Mathf.Pow(a1_y, 2f) + 2 * (a0_x * a2_x + a0_y * a2_y);
        res[1] = 2 * (a0_x * a1_x + a0_y * a1_y);
        res[0] = Mathf.Pow(a0_x, 2f) + Mathf.Pow(a0_y, 2f);
        return res;
    }

    private float DeriveOfDistance(float t)
    {
        float[] distParams = DistanceParams();
        return 4 * Mathf.Pow(t, 3) * distParams[4] + 3 * Mathf.Pow(t, 2) * distParams[3] + 2 * t * distParams[2] + distParams[1];
    }

    private float DeriveOfDeriveOfDistance(float t)
    {
        float[] distParams = DistanceParams();
        return 4 * 3 * Mathf.Pow(t, 2) * distParams[4] + 3 * 2 * t * distParams[3] + 2 * distParams[2];
    }

    public override Curve concat(Curve b)
    {
        if (Algebra.isclose(t_start, b.t_start))
        {
            Bezeir rtn = new Bezeir(P0, P1, P2, z_start + z_offset, b.z_offset - z_offset);
            rtn.t_start = t_end;
            rtn.t_end = b.t_end;
            return rtn;
        }
        if (Algebra.isclose(t_start, b.t_end))
        {
            Bezeir rtn = new Bezeir(P0, P1, P2, z_start + z_start, -b.z_offset - z_offset);
            rtn.t_start = t_end;
            rtn.t_end = b.t_start;
            return rtn;
        }
        if (Algebra.isclose(t_end, b.t_start))
        {
            Bezeir rtn = new Bezeir(P0, P1, P2, z_start, b.z_offset + z_offset);
            rtn.t_start = t_start;
            rtn.t_end = b.t_end;
            return rtn;
        }
        if (Algebra.isclose(t_end, b.t_end))
        {
            Bezeir rtn = new Bezeir(P0, P1, P2, z_start, z_offset - b.z_offset);
            rtn.t_start = t_start;
            rtn.t_end = b.t_start;
            return rtn;
        }
        Debug.Assert(false);
        return null;
    }
}