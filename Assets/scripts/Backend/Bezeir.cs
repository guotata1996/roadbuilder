using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Bezeir : Curve
{
    public Vector2 P0, P1, P2;
    public Bezeir(Vector2 _P0, Vector2 _P1, Vector2 _P2, float _z_start = 0f, float _z_end = 0f)
    {
        Debug.Assert(!Geometry.Parallel(_P1 - _P0, _P2 - _P1));
        P0 = _P0;
        P1 = _P1;
        P2 = _P2;
        z_start = _z_start;
        z_offset = _z_end - _z_start;
        t_start = 0f;
        t_end = 1f;

        Debug.Assert(!Algebra.isclose(this.length, 0f));
    }

    public Bezeir(Vector3 _P0, Vector3 _P1, Vector3 _P2)
    {
        Debug.Assert(!Geometry.Parallel(_P1 - _P0, _P2 - _P1));
        P0 = Algebra.toVector2(_P0);
        P1 = Algebra.toVector2(_P1);
        P2 = Algebra.toVector2(_P2);
        z_start = _P0.y;
        z_offset = _P2.y - _P0.y;
        t_start = 0f;
        t_end = 1f;

        Debug.Assert(!Algebra.isclose(this.length, 0f));
    }

    public override Vector3 at(float t)
    {
        float local_t = t;
        t = toGlobalParam(t);
        float _y = z_start + z_offset * local_t;
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

    /*https://math.stackexchange.com/questions/220900/bezier-curvature*/
    public override float maximumCurvature
    {
        get
        {
            return (P2 + P0 - 2 * P1).magnitude / Mathf.Abs((P1.x - P0.x) * (P2.y - P1.y) - (P1.y - P0.y) * (P2.x - P1.x));
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

    public override Vector3 frontNormal(float t)
    {
        t = t_start + (t_end - t_start) * t;
        Vector2 tangentdir = (2 * (t - 1) * P0 + (2 - 4 * t) * P1 + 2 * t * P2).normalized;
        float tanGradient = z_offset * (t_end - t_start) / this.length;
        return new Vector3(tangentdir.x ,tanGradient, tangentdir.y );
    }

    public override Vector3 rightNormal(float t)
    {
        t = t_start + (t_end - t_start) * t;
        Vector2 tangentdir = (2 * (t - 1) * P0 + (2 - 4 * t) * P1 + 2 * t * P2).normalized;
        return new Vector3(tangentdir.y, 0f, -tangentdir.x);
    }

    public override List<Curve> segmentation(float maxlen)
    {
        List<Curve> result = new List<Curve>();
        float lastEnd = 0;
        int fragCount = Mathf.CeilToInt(this.length / maxlen);
        for (int multipler = 0; multipler < fragCount; multipler++)
        {
            float thisEnd;
            thisEnd = Algebra.newTown(this.lengthFromZeroTo, this.lengthGradient, Mathf.Min(this.length, (float)(multipler + 1) * maxlen), Mathf.Min(1f, maxlen / this.length * (multipler + 1)));

            Bezeir fragment = new Bezeir(this.P0, this.P1, this.P2, this.z_start + this.z_offset * lastEnd, this.z_start + this.z_offset * thisEnd);
            fragment.t_start = toGlobalParam(lastEnd);
            fragment.t_end = toGlobalParam(thisEnd);
            result.Add(fragment);
            lastEnd = thisEnd;
        }
        return result;
    }

    public override float TravelAlong(float currentParam, float distToTravel, bool zeroToOne)
    {
        float currentLength = lengthFromZeroTo(currentParam);
        float targetLength = zeroToOne ? currentLength + distToTravel : currentLength - distToTravel;
        targetLength = Mathf.Clamp(targetLength, 0f, length);
        float newParam = Algebra.newTown(this.lengthFromZeroTo, lengthGradient, targetLength, currentParam);
        return Algebra.approximateTo01(newParam, 1f);
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
            return Algebra.approximateTo01(realparam, length);
    }

    public override string ToString()
    {
        return string.Format("Beizier: t_start={0}, t_end={1}，P0 = {2}, P2 = {3}, ZS = {4}, ZO={5}", t_start, t_end, P0, P2, z_start, z_offset);
    }

    public Vector2 targetP;

    public override Vector3 AttouchPoint(Vector3 p)
    {

        targetP = new Vector2(p.x, p.z);
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

        var sortedParams = candidateParams.OrderBy((param) => (this.at_2d(param) - targetP).magnitude);
        return this.at(sortedParams.First());
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
