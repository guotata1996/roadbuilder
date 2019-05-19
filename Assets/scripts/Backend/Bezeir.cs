using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///   <para>P0 P1 P2 should not be on the same line</para>
/// </summary>
public class Bezeir : Curve
{
    public Vector2 P0
    {
        get;
        private set;
    }
       
    public Vector2 P1
    {
        get;
        private set;
    }

    public Vector2 P2
    {
        get;
        private set;
    }

    public static Curve TryInit(Vector2 _P0, Vector2 _P1, Vector2 _P2)
    {
        if (Algebra.Parallel(_P1 - _P0, _P2 - _P1) || Algebra.isclose(_P0, _P2))
        {
            Debug.LogWarning("Bezeir Ctrl Point Parallel!");
            return Line.TryInit(_P0, _P2);
        }
        return new Bezeir(_P0, _P1, _P2);
    }

    private Bezeir(Vector2 _P0, Vector2 _P1, Vector2 _P2)
    {
        P0 = _P0;
        P1 = _P1;
        P2 = _P2;
        t_start = 0f;
        t_end = 1f;
    }

    protected override float _GetLength()
    {
        return Mathf.Abs(lengthIntegral(t_end) - lengthIntegral(t_start));
    }

    protected override Vector2 _GetFrontDir(float t)
    {
        t = toGlobalParam(t);
        Vector2 global_front = (2 * (t - 1) * P0 + (2 - 4 * t) * P1 + 2 * t * P2).normalized;
        return t_end > t_start ? global_front : -global_front;
    }

    protected override Vector2 _GetTwodPos(float t)
    {
        t = toGlobalParam(t);
        return (1 - t) * (1 - t) * P0 + 2 * t * (1 - t) * P1 + t * t * P2;
    }

    protected override float? _ParamOf(Vector2 p)
    {
        float A11 = (P0.y - 2 * P1.y + P2.y) * 2 * (P1.x - P0.x);
        float A12 = (P0.x - 2 * P1.x + P2.x) * 2 * (P1.y - P0.y);
        float A1 = A11 - A12;

        float A01 = (P0.y - 2 * P1.y + P2.y) * (P0.x - p.x);
        float A02 = (P0.x - 2 * P1.x + P2.x) * (P0.y - p.y);
        float A0 = A01 - A02;

        float realParam = toLocalParam(-A0 / A1);

        if (!Algebra.isclose(GetTwodPos(_ToUnscaledt(realParam)), p))
        {
            return null;
        }
        return Algebra.approximateTo01(realParam, Length);
    }

    protected override float _ToParamt(float unscaled_t)
    {
        return Algebra.newTown(t => _ToUnscaledt(t) * Length, lengthGradient, unscaled_t * Length, 1);
    }

    protected override float _ToUnscaledt(float t)
    {
        float world_start_t = toGlobalParam(0);
        float world_end_t = toGlobalParam(t);
        return Mathf.Sign(t) * Mathf.Abs(lengthIntegral(world_end_t) - lengthIntegral(world_start_t)) / Length;
    }

    float lengthIntegral(float world_t)
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
            Vector2 param_t_coordinate = GetTwodPos(world_t);
            return (param_t_coordinate - P0).magnitude;
        }
        else
        {
            /*http://www.wolframalpha.com/input/?i=integral+sqrt(Ax%5E2%2BBx%2BC) */
            float firstItem = (2 * A * world_t + B) * Mathf.Sqrt(world_t * (A * world_t + B) + C) / (4 * A);
            float secondItem = (B * B - 4 * A * C) * Mathf.Log(2 * Mathf.Sqrt(A) * Mathf.Sqrt(world_t * (A * world_t + B) + C) + 2 * A * world_t + B) / (8 * Mathf.Pow(A, 1.5f));
            return firstItem - secondItem;
        }
    }


    float lengthGradient(float local_t)
    {
        local_t = toGlobalParam(local_t);
        Vector2 dxy_dt = 2 * (local_t - 1) * P0 + (2 - 4 * local_t) * P1 + 2 * local_t * P2;
        return dxy_dt.magnitude / Mathf.Abs(t_end - t_start);
    }

    public override Curve DeepCopy()
    {
        Bezeir copy = new Bezeir(P0, P1, P2);
        copy.t_start = t_start;
        copy.t_end = t_end;
        return copy;
    }

    public override string ToString()
    {
        return "Bezeir P0: " + P0 + " ,P1: " + P1 + " ,P2: " + P2 + 
        " with t_start: " + t_start + " ,t_end = " + t_end;
    }
}
