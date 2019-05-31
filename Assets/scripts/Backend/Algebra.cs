using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public static class Algebra {
    /* real root for a0+a1x+a2x2+a3x3+a4x4=0
     * https://baike.baidu.com/pic/%E4%B8%80%E5%85%83%E5%9B%9B%E6%AC%A1%E6%96%B9%E7%A8%8B%E6%B1%82%E6%A0%B9%E5%85%AC%E5%BC%8F/10721996/0/5d6034a85edf8db1672fae9b0223dd54574e74c8?fr=lemma&ct=single#aid=0&pic=5d6034a85edf8db1672fae9b0223dd54574e74c8
     */

    static float _a0, _a1, _a2, _a3, _a4;

    public static float InfLength = 999f;

    public static float Infinity = 999999f;

    public static float quadFunc(float t)
    {
        return _a4 * Mathf.Pow(t, 4) + _a3 * Mathf.Pow(t, 3) + _a2 * Mathf.Pow(t, 2) + _a1 * t + _a0;
    }

    public static float quadGradient(float t)
    {
        return 4 * _a4 * Mathf.Pow(t, 3) + 3 * _a3 * Mathf.Pow(t, 2) + 2 * _a2 * t + _a1;
    }

    /* Solve function for \Sum{Ai*x^i = 0}*/
    public static float[] functionSolver(float A0, float A1, float A2, float A3 = 0f, float A4 = 0f)
    {
        double[] a = new double[5] { A0, A1, A2, A3, A4 };
        int order = A4 != 0 ? 4 : (A3 != 0 ? 3 : (A2 != 0 ? 2 : 1));
        alglib.polynomialsolve(a, order, out alglib.complex[] res, out alglib.polynomialsolverreport rpt);
        var reals = from resitem in res
                    where resitem.y == 0
                    select (float)resitem.x;
        return reals.ToArray();
    }

    /*b & c*
     * P1 = a2t2+a1t+a0
     * P2 = center + radius * (cost, sint)
     * Guaranteed: Ans in Pi
     */
    public static List<Vector2> parametricFunctionSolver(Vector2 a0, Vector2 a1, Vector2 a2, Vector2 center, float radius)
    {
        Debug.Assert(a2.magnitude > 0);
        float A4 = a2.x * a2.x + a2.y * a2.y;
        float A3 = 2 * a2.x * a1.x + 2 * a2.y * a1.y;
        float A2 = a1.x * a1.x + a1.y * a1.y + 2 * a2.x * (a0.x - center.x) + 2 * a2.y * (a0.y - center.y);
        float A1 = 2 * a1.x * (a0.x - center.x) + 2 * a1.y * (a0.y - center.y);
        float A0 = (a0.x - center.x) * (a0.x - center.x) + (a0.y - center.y) * (a0.y - center.y) - radius * radius;
        var func_1_params = functionSolver(A0, A1, A2, A3, A4);
        var valid_points = from func_1_param in func_1_params
                           select a2 * func_1_param * func_1_param + a1 * func_1_param + a0;
        return valid_points.ToList();
    }

    /*b & l
     * P1 = a2t2+a1t+a0
     * P2 = b0 + b1t
     * Guaranteed: Ans in P1
    */
    public static List<Vector2> parametricFunctionSolver(Vector2 a0, Vector2 a1, Vector2 a2, Vector2 b0, Vector2 b1)
    {
        float A2 = b1.y * a2.x - a2.y * b1.x;
        float A1 = a1.x * b1.y - a1.y * b1.x;
        float A0 = b1.y * (a0.x - b0.x) - b1.x * (a0.y - b0.y);
        var func_1_params = functionSolver(A0, A1, A2);

        //return func_1_params.Where(u => 0 <= u && u <= 1).ToList();
        var valid_points = from func_1_param in func_1_params
                           select a2 * func_1_param * func_1_param + a1 * func_1_param + a0;
        return valid_points.ToList();
    }

    public static float approximateTo01(float p, float baseline = 1f)
    {
        if (isclose(p * baseline, 0f))
            return 0f;
        if (isclose(p * baseline, 1f))
            return 1f;
        return p;
    }

    /*b & b
     * P1 = a2t2 + a1t1 + a0
     * P2 = b2t2 + b1t1 + b0
    */
    public static List<Vector2> parametricFunctionSolver(Vector2 a0, Vector2 a1, Vector2 a2, Vector2 b0, Vector2 b1, Vector2 b2)
    {
        float C = a2.x * b2.y - b2.x * a2.y;
        float D = a1.x * b2.y - b2.x * a1.y;
        float E = b2.y * (a0.x - b0.x) - b2.x * (a0.y - b0.y);
        float F = b1.x * b2.y - b1.y * b2.x;
        float A0, A1, A2, A3, A4;


        if (Mathf.Approximately(b2.x, 0f))
        {
            A4 = C * C * b2.y;
            A3 = 2 * C * D * b2.y;
            A2 = (D * D + 2 * C * E) * b2.y + C * F * b1.y - F * F * a2.y;
            A1 = 2 * D * E * b2.y + D * F * b1.y - F * F * a1.y;
            A0 = E * E * b2.y + E * F * b1.y + F * F * b0.y - F * F * a0.y;
        }
        else{
            A4 = C * C * b2.x;
            A3 = 2 * C * D * b2.x;
            A2 = (D * D + 2 * C * E) * b2.x + C * F * b1.x - F * F * a2.x;
            A1 = 2 * D * E * b2.x + D * F * b1.x - F * F * a1.x;
            A0 = E * E * b2.x + E * F * b1.x + F * F * b0.x - F * F * a0.x;
        }
        var func_params = functionSolver(A0, A1, A2, A3, A4);

        IEnumerable<Vector2> valid_points;
        if (Mathf.Approximately(b2.x, 0f))
        {
            valid_points = from func_2_param in func_params
                           select b2 * func_2_param * func_2_param + b1 * func_2_param + b0;
        }
        else{
            valid_points = from func_1_param in func_params
                           select a2 * func_1_param * func_1_param + a1 * func_1_param + a0;
        }
        return valid_points.ToList();
    }

    /*c & l
     * P1 = a0 + a1t
     * P2 = center + radius * (cost, sint)
     */
     public static List<Vector2> parametricFunctionSolver(Vector2 a0, Vector2 a1, Vector2 center, float radius)
    {
        float A2 = a1.x * a1.x + a1.y * a1.y;
        float A1 = 2 * a1.x * (a0.x - center.x) + 2 * a1.y * (a0.y - center.y);
        float A0 = (a0.x - center.x) * (a0.x - center.x) + (a0.y - center.y) * (a0.y - center.y) - radius * radius;
        var func_1_params = functionSolver(A0, A1, A2);
        var points = from func_1_param in func_1_params
                     select a0 + a1 * func_1_param;
        return points.ToList();
    }

    /*c & c
     * center should not overlap
     * P1 = c1 + r1*(cost, sint)
     * P2 = c2 + r2*(cost, sint)
     */ 
     public static List<Vector2> parametricFunctionSolver(Vector2 center1, float radius1, Vector2 center2, float radius2)
    {
        Debug.Assert((center1 - center2).magnitude > 0);
        float center_dist = (center1 - center2).magnitude;
        float A = Mathf.Pow(center_dist, 2f);
        float C1 = (Mathf.Pow(radius2, 2f) - A - Mathf.Pow(radius1, 2)) / (2 * radius1);
        float B = -2 * C1 * (center1.y - center2.y);
        float C = Mathf.Pow(C1, 2f) - Mathf.Pow(center1.x - center2.x, 2f);
        var func_1_params_sined = functionSolver(C, B, A);
        var points = from func_1_param_sined in func_1_params_sined
                     let func_1_params_cosed = new List<float> { Mathf.Sqrt(1 - func_1_param_sined * func_1_param_sined),  -Mathf.Sqrt(1 - func_1_param_sined * func_1_param_sined) }
                        from func_1_param_cosed in func_1_params_cosed
                        let px = center1.x + func_1_param_cosed * radius1
                        let py = center1.y + func_1_param_sined * radius1
                        let P = new Vector2(px, py)
                        where isclose((P - center2).magnitude, radius2)
                        select P;
        return points.ToList();
    }

    /*l & l
     * P1 = a0 + a1t
     * P2 = b0 + b1t
    */
    public static List<Vector2> parametricFunctionSolver(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1)
    {
        float A1 = b1.x * a1.y - a1.x * b1.y;
        if (A1 == 0){
            //parallel
            return new List<Vector2>();
        }
        float A0 = - (a0.x * b1.y - a0.y * b1.x + b0.y * b1.x - b0.x * b1.y);
        var func_1_params = functionSolver(A0, A1, 0f);
        var points = from func_1_param in func_1_params
                     select a0 + a1 * func_1_param;
        return points.ToList();
    }

    public static bool isclose(float a, float b, float baseline = 1)
    {
        return Mathf.Abs(a - b) * baseline < 1e-3;
    }

    public static bool isclose(double a, double b)
    {
        return Math.Abs(a - b) < 1e-3;
    }

    public static bool isclose(Vector2 a, Vector2 b, float baseline = 1f){
        return isclose((a - b).magnitude, 0f, baseline);
    }

    public static bool isclose(Vector3 a, Vector3 b)
    {
        return isclose((a - b).magnitude, 0f);
    }

    public static bool isProjectionClose(Vector3 a, Vector3 b)
    {
        return isclose((new Vector2(a.x, a.z) - new Vector2(b.x, b.z)).magnitude, 0f);
    }

    public static bool isRoadNodeClose(Vector2 a, Vector2 b){
        return Mathf.Abs((a - b).magnitude) < 0.1f;
    }

    public static bool isRoadNodeClose(Vector3 a, Vector3 b)
    {
        return Mathf.Abs((a - b).magnitude) < 0.1f;
    }

    public static bool approximatelySmaller(float a, float b)
    {
        return a < b + 1e-4;
    }

    public delegate float Del(float x);

    /*sove function: f(x) = targetValue, f' = gradient*/
    public static float NewTown(Del function, Del gradient, float targetValue, float startValue = 1f)
    {
        float ans = startValue;
        float error_baseLine = Mathf.Max(function(0f), function(1f), targetValue, function(startValue));

        for(int i = 0; i != 50; ++i)
        {
            ans = ans - (function(ans) - targetValue) / gradient(ans);
            if (isclose(function(ans), targetValue, error_baseLine)){
                return ans;
            }
        }

        //throw new Exception("Newton method low precesion!");
        return float.NegativeInfinity;
    }

    public static float BinaryApproach(Del monoFunction, float targetValue, float startValue = 0f){
        while (monoFunction(startValue) < targetValue){
            startValue += 0.01f;
        }
        float hi = startValue, lo = startValue - 0.01f;
        float mid = 0.5f * (lo + hi);
        while (!isclose(targetValue, monoFunction(mid))){
            mid = 0.5f * (hi + lo);
            if (monoFunction(mid) > targetValue){
                hi = mid;
            }
            else{
                lo = mid;
            }
        }
        return mid;
    }

    public static float MinArg(Del function, float startValue, 
    float lowerLimit = float.NegativeInfinity, float upperLimit = float.PositiveInfinity){
        float stepBase = 0.1f;
        int stepCnt = 0;

        while (stepBase > 1e-6 && stepCnt < 1e5)
        {
            float step = (function(startValue) < function(startValue + stepBase)) ? -stepBase : stepBase;

            while (function(startValue) > function(startValue + step) && 
            lowerLimit <= startValue + step && startValue + step <= upperLimit)
            {
                startValue += step;
                stepCnt++;
            }
            stepBase *= 0.1f;
        }
        return startValue;
    }

    public static Vector3 approximate(Vector3 precise){
        return new Vector3(Mathf.RoundToInt(precise.x), Mathf.RoundToInt(precise.y), Mathf.RoundToInt(precise.z));
    }

    public static Vector2 approximate(Vector2 precise)
    {
        return new Vector2(Mathf.RoundToInt(precise.x), Mathf.RoundToInt(precise.y));
    }

    public static Vector2 angle2dir(float angle){
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    public static Vector3 angle2dir_3d(float angle){
        return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
    }

    public static float signedAngleToPositive(float angle)
    {
        return angle >= 0 ? angle : angle + 360f;
    }

    public static float twodCross(Vector2 a, Vector2 b){
        return a.x * b.y - a.y * b.x;
    }

    public static Vector2 twodRotate(Vector2 a, float angle){
        return new Vector2(Mathf.Cos(angle) * a.x - Mathf.Sin(angle) * a.y, Mathf.Sin(angle) * a.x + Mathf.Cos(angle) * a.y);
    }

    public static Vector3 toVector3(Vector2 a, float y = 0f){
        return new Vector3(a.x, y, a.y);
    }

    public static Vector2 toVector2(Vector3 a)
    {
        return new Vector2(a.x, a.z);
    }

    public static float Lerp(float a, float b, float t){
        return t * b + (1 - t) * a;
    }

    public static Vector3 unitVecInterpolator(Vector3 a, Vector3 b, float s, float t)
    {
        return ((a * t + b * s) / (s + t)).normalized;
    }

    public static Vector3 genericVectIntepolator(Vector3 a, Vector3 b, float s, float t)
    {
        return (a * t + b * s) / (s + t);
    }

    public static float genericScalarIntepolator(float a, float b, float s, float t)
    {
        return ((a * t + b * s)) / (s + t);
    }

    public static Vector2 RotatedY(Vector2 input, float theta)
    {
        float x = Mathf.Cos(theta) * input.x - Mathf.Sin(theta) * input.y;
        float y = Mathf.Sin(theta) * input.x + Mathf.Cos(theta) * input.y;
        return new Vector2(x, y);
    }

    public static bool Parallel(Vector2 line1, Vector2 line2)
    {
        return (Algebra.isclose(line1.x, 0) && Algebra.isclose(line2.x, 0))
            || Algebra.isclose(line1, Vector2.zero) || Algebra.isclose(line2, Vector2.zero) ||
                    Algebra.isclose(line1.y / line1.x, line2.y / line2.x);
    }

    /// <summary>
    /// Projects point on ray
    /// </summary>
    /// <param name="rayDir">Ray dir. Must be normalized</param>
    public static Vector2 ProjectOn(Vector2 point, Vector2 rayPoint, Vector2 rayDir)
    {
        return rayPoint + Vector2.Dot(point - rayPoint, rayDir) * rayDir;
    }
    
}