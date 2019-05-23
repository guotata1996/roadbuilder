using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Curve3DSampler : IEnumerator
{
    public float StepSize
    {
        get
        {
            if (_designatedStepSize >= 0f)
            {
                return _designatedStepSize;
            }
            return _preferredSampleRealResolution / (xz_curve.Length * xz_curve.GetMaximumCurvature);
        }
        set
        {
            _designatedStepSize = value;
        }
    }

    // Ordered by importance!
    float _designatedStepSize = -0.1f; // Fixed. Only used if value >= 0
    float _preferredSampleRealResolution = 0f; // real world length; StepSize changes with curve Length

    Curve xz_curve;
    Function y_func;

    public Curve3DSampler(Curve xz_source, Function y_source, float sampleRealResolution = 0f)
    {
        _preferredSampleRealResolution = sampleRealResolution;
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
        if (Algebra.isclose(cursor, 1.0f, xz_curve.Length))
        {
            return false;
        }
        cursor = Mathf.Clamp01(cursor + StepSize);
        return true;
    }

    public void Reset()
    {
        cursor = Mathf.NegativeInfinity;
    }

    protected bool IsValid
    {
        get
        {
            return xz_curve.IsValid && y_func.IsValid;
        }
    }

    public List<Vector3> ControlPoints
    {
        get
        {
            var xz_ctrl = xz_curve.ControlPoints;
            var rtn = new List<Vector3>(xz_ctrl.Count);
            for (int i = 0; i != rtn.Count; ++i)
            {
                if (i == 0)
                {
                    rtn[i] = Algebra.toVector3(xz_ctrl[i], y_func.ControlPoints[0]);
                }
                else
                {
                    if (i == rtn.Count - 1)
                    {
                        rtn[i] = Algebra.toVector3(xz_ctrl[i], y_func.ControlPoints[1]);
                    }
                    else
                    {
                        rtn[i] = Algebra.toVector3(xz_ctrl[i]);
                    }
                }
            }
            return rtn;
        }
        set
        {
            xz_curve.ControlPoints = value.ConvertAll(Algebra.toVector2);

            if (y_func is LinearFunction) {
                ((LinearFunction)y_func).ControlPoints = new List<float>(
                new float[]{ value[0].y, value[value.Count - 1].y});
            }
        }
    }
}
