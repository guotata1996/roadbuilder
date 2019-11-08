using UnityEngine;
using System;

[System.Serializable]
public class Bezier
{
    Vector2 p0, p1, p2;
    Vector2 hp0, hp1, hp2, hp3;
    bool isStraight;

    const int subDiv = 24;
    /*length of projection on XZ plane*/
    public float curveLength;
    public float[] paramToRealLength;

    private Bezier(Vector2 _p0, Vector2 _p1, Vector2 _p2, float _h0, float _h2, float _t0, float _t2) {
        p0 = _p0;
        p1 = _p1;
        p2 = _p2;

        isStraight = false;

        #region init_length
        paramToRealLength = new float[subDiv + 1];
        paramToRealLength.SetValue(0, 0);
        curveLength = 0f;
        Vector2 lastPoint = Vector2.zero;
        for (int i = 0; i != subDiv + 1; ++i)
        {
            float t = ((float)i) / subDiv;
            Vector2 currPoint = Math3d.GetPointOnSpline(t, new Vector2[] { p0, p1, p2 });
            if (i != 0)
            {
                curveLength += (lastPoint - currPoint).magnitude;
                paramToRealLength.SetValue(curveLength, i);
            }
            lastPoint = currPoint;
        }

        #endregion

        hp1 = new Vector2(0, _h0);
        hp2 = new Vector2(curveLength, _h2);
        hp0 = new Vector2(-curveLength, _h2 - _t0 * 2 * curveLength);
        hp3 = new Vector2(2 * curveLength, _h0 + _t2 * 2 * curveLength);
    }

    float GetParamOfPercentage(float percentage){
        float realLength = percentage * curveLength;
        if (realLength < 0.001f){
            return 0;
        }
        if (realLength > curveLength - 0.001f){
            return 1;
        }
        int upIndex = Array.BinarySearch(paramToRealLength, realLength); // [1...SubDiv]
        if (upIndex < 0){
            upIndex = ~upIndex;
        }
        float upIndexLength = (float)paramToRealLength.GetValue(upIndex);
        float loIndexLength = (float)paramToRealLength.GetValue(upIndex - 1);
        float loIndexWeight = upIndexLength - realLength;
        float upIndexWeight = realLength - loIndexLength;
        float rtn = Mathf.Lerp(((float)(upIndex - 1)) / subDiv, ((float)upIndex) / subDiv, 
        upIndexWeight / (loIndexWeight + upIndexWeight));
        return rtn;

    }

    private Bezier(Vector2 _from, Vector2 _to, float _h0, float _h2, float _t0, float _t2)
    {
        p0 = _from;
        p2 = _to;

        isStraight = true;
        curveLength = Vector3.Distance(p0, p2);

        hp1 = new Vector2(0, _h0);
        hp2 = new Vector2(curveLength, _h2);
        hp0 = new Vector2(-curveLength, _h2 - _t0 * 2 * curveLength);
        hp3 = new Vector2(2 * curveLength, _h0 + _t2 * 2 * curveLength);
    }

    public static Bezier Create(Ray from, Ray to)
    {
        Vector3 middle;
        Ray projectedFrom = new Ray(new Vector3(from.origin.x, 0f, from.origin.z), Vector3.ProjectOnPlane(from.direction, Vector3.up));
        Ray projectedTo = new Ray(new Vector3(to.origin.x, 0f, to.origin.z), Vector3.ProjectOnPlane(-to.direction, Vector3.up));

        float directionHFrom = from.direction.y / new Vector2(from.direction.x, from.direction.z).magnitude;
        float directionHTo = to.direction.y / new Vector2(to.direction.x, to.direction.z).magnitude;

        if (Math3d.RayRayIntersection(out middle, projectedFrom, projectedTo))
        {
            return new Bezier(new Vector2(from.origin.x, from.origin.z), new Vector2(middle.x, middle.z), new Vector2(to.origin.x, to.origin.z),
                from.origin.y, to.origin.y, directionHFrom, directionHTo);
        }
        else
        {
            if (Vector3.Dot(projectedFrom.direction, projectedTo.direction) < -0.999f && Vector3.Dot((projectedTo.origin - projectedFrom.origin).normalized, projectedFrom.direction) > 0.999f)
            {
                return new Bezier(new Vector2(from.origin.x, from.origin.z), new Vector2(to.origin.x, to.origin.z), from.origin.y, to.origin.y, directionHFrom, directionHTo);
            }
            return null;
        }
    }

    public Vector3 GetPoint(float t)
    {
        Vector2 Y_point = Math3d.GetPointOnSpline(t, new Vector2[] { hp0, hp1, hp2, hp3 });
        Vector2 XZ_point;
        if (isStraight)
        {
            XZ_point = Math3d.GetPointOnSpline(t, new Vector2[] { p0, p2}); 
        }
        else
        {
            t = GetParamOfPercentage(t);
            XZ_point = Math3d.GetPointOnSpline(t, new Vector2[] { p0, p1, p2 });  
        }
        return new Vector3(XZ_point.x, Y_point.y, XZ_point.y);
    }

    public Vector3 GetForward(float t)
    {
        if (!isStraight)
        {
            t = GetParamOfPercentage(t);
        }
        Vector3 tmp1 = GetPoint(Mathf.Max(t - 0.05f, 0f));
        Vector3 tmp2 = GetPoint(Mathf.Min(t + 0.05f, 1f));

        return (tmp2 - tmp1).normalized;
    }

    public Vector3 GetRight(float t)
    {
        if (!isStraight)
            t = GetParamOfPercentage(t);
        return Vector3.Cross(Vector3.up, GetForward(t)).normalized;
    }

}
