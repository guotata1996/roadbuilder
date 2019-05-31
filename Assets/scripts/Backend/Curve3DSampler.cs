using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Curve3DSampler : LinearFragmentable<Curve3DSampler>, IEnumerator
{
    protected event System.EventHandler OnShapeChanged;

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
    
    Curve _xz_curve;

    void PassShapeChangedNotice(object sender, System.EventArgs e)
    {
        OnShapeChanged?.Invoke(this, null);
    }


    public Curve xz_curve
    {
        get
        {
            return _xz_curve;
        }
        set
        {
            // Unsubscribe from old curve
            if (_xz_curve != null)
            {
                _xz_curve.OnShapeChanged -= PassShapeChangedNotice;
            }
            _xz_curve = value;
            _xz_curve.OnShapeChanged += PassShapeChangedNotice;
            // Invoke immediately
            OnShapeChanged?.Invoke(this, null);
        }
    }

    Function _y_func;
    public Function y_func
    {
        get
        {
            return _y_func;
        }
        set
        {
            // Unsubscribe from old func
            if (_y_func != null)
            {
                _y_func.OnValueChanged -= PassShapeChangedNotice;
            }
            _y_func = value;
            _y_func.OnValueChanged += PassShapeChangedNotice;
            // Invoke immediately
            OnShapeChanged?.Invoke(this, null);
        }
    }

    public Curve3DSampler(Curve xz_source, Function y_source, float sampleRealResolution = 0f)
    {
        _preferredSampleRealResolution = sampleRealResolution;
        xz_curve = xz_source;
        y_func = y_source;

        Reset();
    }

    float cursor;

    public Vector3 GetThreedPos(float t)
    {
        Vector2 pos_xz = xz_curve.GetTwodPos(t);
        float pos_y = y_func.ValueAt(t);
        return new Vector3(pos_xz.x, pos_y, pos_xz.y);
    }

    public Vector3 GetThreedPos(Vector2 twodPos)
    {
        var t = xz_curve.ParamOf(twodPos).Value;
        return Algebra.toVector3(twodPos, y_func.ValueAt(t));
    }

    /// <summary>
    /// If difference in Y is less than 2, we consider 
    /// </summary>
    public List<Vector3> IntersectWith(Curve3DSampler another, bool filter_self = true, bool filter_other = true)
    {
        var two_d_candidates = xz_curve.IntersectWith(another.xz_curve, filter_self, filter_other);

        return two_d_candidates.FindAll
        (twodPos =>
            (this.GetThreedPos(twodPos) - another.GetThreedPos(twodPos)).sqrMagnitude < 2 * 2
        ).ConvertAll(GetThreedPos);
    }

    /// <summary>
    /// Tuple (pos, right, front)
    /// </summary>
    public object Current
    {
        get
        {
            Vector3 pos = GetThreedPos(cursor);

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

    public bool IsValid
    {
        get
        {
            return xz_curve.IsValid && y_func.IsValid 
            && !float.IsInfinity(StepSize) && StepSize != 0f;
        }
    }

    public Vector3 GetAttractedPoint(Vector3 p, float attract_radius)
    {
        Vector2 p_2d = Algebra.toVector2(p);
        Vector2 attracted_p_2d = xz_curve.GetAttractedPoint(p_2d, attract_radius);
        if (attracted_p_2d == p_2d)
        {
            // exceed radius
            return p;
        }
        float y = y_func.ValueAt(xz_curve.ParamOf(attracted_p_2d).Value);
        Vector3 candidate = Algebra.toVector3(attracted_p_2d, y);
        if ((candidate - p).sqrMagnitude <= attract_radius * attract_radius)
        {
            return Algebra.toVector3(attracted_p_2d, y);
        }

        return p;
    }

    public override Curve3DSampler Clone()
    {
        return new Curve3DSampler(xz_curve.Clone(), y_func.Clone(), _preferredSampleRealResolution)
        {
            _designatedStepSize = this._designatedStepSize
        };
    }

    public override void Crop(float unscaled_t_start, float unscaled_t_end)
    {
        xz_curve.Crop(unscaled_t_start, unscaled_t_end);
        y_func.Crop(unscaled_t_start, unscaled_t_end);

        Reset();
    }

    public List<Vector3> ControlPoints
    {
        get
        {
            var xz_ctrl = xz_curve.ControlPoints;
            var rtn = new List<Vector3>();
            for (int i = 0; i != xz_ctrl.Count; ++i)
            {
                if (i == 0)
                {
                    rtn.Add(Algebra.toVector3(xz_ctrl[i], y_func.ControlPoints[0]));
                }
                else
                {
                    if (i == rtn.Count - 1)
                    {
                        rtn.Add(Algebra.toVector3(xz_ctrl[i], y_func.ControlPoints[1]));
                    }
                    else
                    {
                        rtn.Add(Algebra.toVector3(xz_ctrl[i]));
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
