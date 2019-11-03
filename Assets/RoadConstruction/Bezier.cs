using UnityEngine;
using System;

[System.Serializable]
public class Bezier
{
    Vector3 p0, p1, p2;
    bool isStraight;

    const int subDiv = 24;
    public float curveLength;
    public float[] paramToRealLength;

    private Bezier(Vector3 _p0, Vector3 _p1, Vector3 _p2) {
        p0 = _p0;
        p1 = _p1;
        p2 = _p2;
        isStraight = false;

        #region init_length
        paramToRealLength = new float[subDiv + 1];
        paramToRealLength.SetValue(0, 0);
        curveLength = 0f;
        Vector3 lastPoint = Vector3.zero;
        for (int i = 0; i != subDiv + 1; ++i)
        {
            float t = ((float)i) / subDiv;
            Vector3 currPoint = Math3d.GetPointOnSpline(t, new Vector3[] { p0, p1, p2 });
            if (i != 0)
            {
                curveLength += (lastPoint - currPoint).magnitude;
                paramToRealLength.SetValue(curveLength, i);
            }
            lastPoint = currPoint;
        }

        #endregion
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

    private Bezier(Vector3 _from, Vector3 _to)
    {
        p0 = _from;
        p2 = _to;
        isStraight = true;
        curveLength = Vector3.Distance(p0, p2);
    }

    public static Bezier Create(Ray from, Ray to)
    {
        Vector3 middle;
        Ray reversedTo = new Ray(to.origin, -to.direction);
        if (Math3d.RayRayIntersection(out middle, from, reversedTo))
        {
            return new Bezier(from.origin, middle, to.origin);
        }
        else
        {
            if (Vector3.Dot(from.direction, to.direction) > 0.999f && Vector3.Dot((to.origin - from.origin).normalized, from.direction) > 0.999f)
            {
                return new Bezier(from.origin, to.origin);
            }
            return null;
        }
    }

    public Vector3 GetPoint(float t)
    {
        if (!isStraight)
            t = GetParamOfPercentage(t);
        if (isStraight)
        {
            return Math3d.GetPointOnSpline(t, new Vector3[] { p0, p2});
        }
        else
        {
            return Math3d.GetPointOnSpline(t, new Vector3[] { p0, p1, p2 });
        }
    }

    public Vector3 GetForward(float t)
    {
        if (isStraight){
            return (p2-p0).normalized;
        }
        else
            t = GetParamOfPercentage(t);

        
        Vector3 tmp1 = GetPoint(t - 0.05f);
        Vector3 tmp2 = GetPoint(t + 0.05f);
        return (tmp2 - tmp1).normalized;
    }

    public Vector3 GetRight(float t)
    {
        if (!isStraight)
            t = GetParamOfPercentage(t);
        return Vector3.Cross(Vector3.up, GetForward(t)).normalized;
    }

}
