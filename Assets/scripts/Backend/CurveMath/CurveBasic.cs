using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class Curve : LinearFragmentable<Curve>, ITwodPosAvailable
{
    protected float t_start, t_end;
    protected Vector2[] controlPoints;

    public event System.EventHandler<int> OnShapeChanged;

    /// <summary>
    /// Gets a copy of control points.
    /// </summary>
    public abstract List<Vector2> ControlPoints { get; set; }

    /// <summary>
    /// 1. Update internal buffered values
    /// 2. Notify holding objects to repaint
    /// </summary>
    protected virtual void NotifyShapeChanged()
    {
        _length = null;
        OnShapeChanged?.Invoke(this, 0);
    }

    /*<<<<<<<<<<<<<<<<<<<<<<<<<<<<>>>>>>>>>>>>>>>>>>>>>>>>>>>*/
    /*Basic Methods */
    public abstract bool IsValid { get; }

    public Vector2 GetTwodPos(float unscaled_t){
        return _GetTwodPos(_ToParamt(unscaled_t));
    }
    public Vector2 GetFrontDir(float unscaled_t){
        return _GetFrontDir(_ToParamt(unscaled_t));
    }

    /// <summary>
    ///   <para>Range: (-PI,PI]</para>
    /// </summary>
    public float GetFrontAngle(float unscaled_t){
        Vector2 direction = GetFrontDir(unscaled_t);
        return Mathf.Atan2(direction.y, direction.x);
    }

    public Vector2 GetRightDir(float unscaled_t){
        return _GetRightDir(_ToParamt(unscaled_t));
    }

    public abstract float GetMaximumCurvature { get; }

    /// <summary>
    /// Attracts the p to curve. Default implementation uses a binary search
    /// </summary>
    /// <returns>If the curve is not in radius distance, then just return p</returns>
    public virtual Vector2 GetAttractedPoint(Vector2 p, float attract_radius)
    {
        Algebra.Del _Distance = (t) => (_GetTwodPos(t) - p).sqrMagnitude;
        float close_scaled_t = Algebra.MinArg(_Distance, 0.5f, 0f, 1f);
        Vector2 closest_p = _GetTwodPos(close_scaled_t);
        if ((closest_p - p).sqrMagnitude <= attract_radius * attract_radius)
        {
            return closest_p;
        }
        else
        {
            return p;
        }
    }

    /// <summary>
    ///   <para>Could return unscaled params outside (0,1). 
    ///   But approx to 0/1 if possible</para>
    /// </summary>
    public float? ParamOf(Vector2 p){
        float? _param = _ParamOf(p);

        if (_param != null){
            return _ToUnscaledt(_param.Value);
        }
        else{
            return null;
        }
    }

    public bool Contains(Vector2 p){
        float? _param = _ParamOf(p);
        if (_param == null){
            return false;
        }
        else{
            return 0f <= _param.Value && _param.Value <= 1f;
        }
    }


    public abstract float _ToParamt(float unscaled_t);
    /*Should also handle negative case */
    public abstract float _ToUnscaledt(float t);
    protected abstract Vector2 _GetTwodPos(float t);
    protected abstract Vector2 _GetFrontDir(float t);
    protected Vector2 _GetRightDir(float t)
    {
        return Algebra.RotatedY(_GetFrontDir(t), -Mathf.PI / 2);
    }

    protected abstract float? _ParamOf(Vector2 p);

    protected float toGlobalParam(float t)
    {
        return t_start + t * (t_end - t_start);
    }

    protected float toLocalParam(float global_t)
    {
        return (global_t - t_start) / (t_end - t_start);
    }

    /*<<<<<<<<<<<<<<<<<<<<<<<<<<<<>>>>>>>>>>>>>>>>>>>>>>>>>>>*/
    /*Buffered Methods */
    float? _length;
    protected abstract float _GetLength();

    public float Length{
        get{
            if (_length == null){
                _length = _GetLength();
            }
            return _length.Value;
        }
    }

    /*<<<<<<<<<<<<<<<<<<<<<<<<<<<<>>>>>>>>>>>>>>>>>>>>>>>>>>>*/

    /*Common override methods */
    public override void Crop(float unscaled_t_start, float unscaled_t_end)
    {
        float scaled_t_start = _ToParamt(unscaled_t_start);
        float scaled_t_end = _ToParamt(unscaled_t_end);
        float new_t_start = toGlobalParam(scaled_t_start);
        float new_t_end = toGlobalParam(scaled_t_end);
        t_start = new_t_start;
        t_end = new_t_end;

        NotifyShapeChanged();
    }

}
