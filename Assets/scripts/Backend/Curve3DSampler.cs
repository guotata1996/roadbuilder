using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Curve3DSampler : IEnumerator
{
    public float stepSize;
    Curve xz_curve;
    Function y_func;

    public Curve3DSampler(Curve xz_source, Function y_source, float sampleRealResolution = 0f)
    {
        stepSize = sampleRealResolution / xz_source.Length;
        xz_curve = xz_source;
        y_func = y_source;
    }

    float cursor;

    /// <summary>
    /// Tuple (pos, right, front)
    /// </summary>
    public object Current
    {
        get
        {
            Vector2 pos_xz = xz_curve.GetTwodPos(cursor);
            float pos_y = y_func.ValueAt(cursor);
            var pos = new Vector3(pos_xz.x, pos_y, pos_xz.y);

            Vector2 right_xz = xz_curve.GetRightDir(cursor);
            var right = Algebra.toVector3(right_xz);

            Vector2 front_xz = xz_curve.GetFrontDir(cursor);
            float tangent = y_func.GradientAt(cursor) / xz_curve.Length;
            var front = new Vector3(front_xz.x, tangent, front_xz.y).normalized;
            return (pos, right, front);
        }
    }

    public bool MoveNext()
    {
        if (cursor == 1f)
        {
            return false;
        }
        cursor = Mathf.Clamp01(cursor + stepSize);
        return true;
    }

    public void Reset()
    {
        cursor = -stepSize;
    }
}
