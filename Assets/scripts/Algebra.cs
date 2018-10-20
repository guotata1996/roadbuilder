using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class Algebra {
    /* real root for a0+a1x+a2x2+a3x3+a4x4=0
     * https://baike.baidu.com/pic/%E4%B8%80%E5%85%83%E5%9B%9B%E6%AC%A1%E6%96%B9%E7%A8%8B%E6%B1%82%E6%A0%B9%E5%85%AC%E5%BC%8F/10721996/0/5d6034a85edf8db1672fae9b0223dd54574e74c8?fr=lemma&ct=single#aid=0&pic=5d6034a85edf8db1672fae9b0223dd54574e74c8
     */

    static float _a0, _a1, _a2, _a3, _a4;

    public static float InfLength = 99999f;

    public static List<float> functionSolver(float a0, float a1, float a2, float a3 = 0, float a4 =0)
    {
        List<float> ans = new List<float>();
        if (a4 != 0)
        {
            _a0 = a0;
            _a1 = a1;
            _a2 = a2;
            _a3 = a3;
            _a4 = a4;
            float step = 0.01f;
            for (float p = 0;  p <= 1f - step;  p += step)
            {
                if (quadFunc(p) * quadFunc(p + step) <= 0)
                {
                    if (!ans.Contains(p))
                        ans.Add(newTown(quadFunc, quadGradient, 0f, startValue:p));
                }

            }


            /*
            double delta1 = a2 * a2 - 3 * a3 * a1 + 12 * a4 * a0;
            double delta2 = 2 * a2 * a2 * a2 - 9 * a3 * a2 * a1 + 27 * a4 * a1 * a1 + 27 * a3 * a3 * a0 - 72 * a4 * a2 * a0;
            double delta_down = Math.Pow(delta2 + Math.Sqrt(-4 * Math.Pow(delta1, 3) + Math.Pow(delta2, 2)), 1f / 3);
            double delta = Math.Pow(2f, 1f / 3) * delta1 / (3 * a4 * delta_down) + delta_down / (3 * Math.Pow(2f, 1f / 3) * a4);
            double extend_delta_first = Math.Pow(a3, 2) / (4 * Math.Pow(a4, 2)) - (2 * a2) / (3 * a4) + delta;
            Debug.Log(extend_delta_first);

            double extended_delta_second_right_up = -Math.Pow((a3 / a4), 3) + 4 * a3 * a2 / (a4 * a4) - 8 * a1 / a4;
            double extended_delta_second_1;
            double extended_delta_second_2;
            if (extend_delta_first > 0)
            {
                extend_delta_first = Math.Sqrt(extend_delta_first);
                extended_delta_second_1 = Math.Pow(a3, 2f) / (2 * Math.Pow(a4, 2f)) - (4 * a2) / (3 * a4) - delta - extended_delta_second_right_up / (4 * extend_delta_first);
                extended_delta_second_2 = Math.Pow(a3, 2f) / (2 * Math.Pow(a4, 2f)) - (4 * a2) / (3 * a4) - delta + extended_delta_second_right_up / (4 * extend_delta_first);
            }
            else
            {
                extend_delta_first = 0f;
                extended_delta_second_1 = extended_delta_second_2 = Math.Pow(a3, 2f) / (2 * Math.Pow(a4, 2f)) - (4 * a2) / (3 * a4) - delta;
                Debug.Log(extended_delta_second_1);
            }
                

            if (isclose(extended_delta_second_1, 0))
            {
                ans.Add((float)(-a3 / (4 * a4) - extend_delta_first / 2));
            }
            else
            if (extended_delta_second_1 > 0)
            {
                ans.Add((float)(-a3 / (4 * a4) - extend_delta_first / 2 - Math.Sqrt(extended_delta_second_1) / 2));
                ans.Add((float)(-a3 / (4 * a4) - extend_delta_first / 2 + Math.Sqrt(extended_delta_second_1) / 2));
            }

            if (isclose(extended_delta_second_2, 0))
            {
                ans.Add((float)(-a3 / (4 * a4) + extend_delta_first / 2));
            }
            else
            if (extended_delta_second_2 > 0)
            {
                ans.Add((float)(-a3 / (4 * a4) + extend_delta_first / 2 - Math.Sqrt(extended_delta_second_2) / 2));
                ans.Add((float)(-a3 / (4 * a4) + extend_delta_first / 2 + Math.Sqrt(extended_delta_second_2) / 2));
            }
            */
        }

        else
        {
            Debug.Assert(a3 == 0);
            return secondLevelfunctionSolver((float)a0, (float)a1, (float)a2);
        }
        return ans;
        
    }

    public static float quadFunc(float t)
    {
        return _a4 * Mathf.Pow(t, 4) + _a3 * Mathf.Pow(t, 3) + _a2 * Mathf.Pow(t, 2) + _a1 * t + _a0;
    }

    public static float quadGradient(float t)
    {
        return 4 * _a4 * Mathf.Pow(t, 3) + 3 * _a3 * Mathf.Pow(t, 2) + 2 * _a2 * t + _a1;
    }

    private static List<float> secondLevelfunctionSolver(float a0, float a1, float a2)
    {
        List<float> ans = new List<float>();
        if (a2 != 0)
        {
            float delta = a1 * a1 - 4 * a0 * a2;
            if (delta > 0)
            {
                ans.Add((-a1 + Mathf.Sqrt(delta)) / (2 * a2));
                ans.Add((-a1 - Mathf.Sqrt(delta)) / (2 * a2));
            }
            if (delta == 0)
            {
                ans.Add(-a1 / (2 * a2));
            }
        }
        else
        {
            if (a1 != 0)
            {
                ans.Add(-a0 / a1);
            }
        }
        return ans;
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
        List<float> func_1_params = functionSolver(A0, A1, A2, A3, A4);
        var valid_points = from func_1_param in func_1_params
                           where 0 <= func_1_param && func_1_param <= 1
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
        List<float> func_1_params = functionSolver(A0, A1, A2);

        //return func_1_params.Where(u => 0 <= u && u <= 1).ToList();
        var valid_points = from func_1_param in func_1_params
                           select a2 * func_1_param * func_1_param + a1 * func_1_param + a0;
        return valid_points.ToList();
    }

    internal static float? approximateTo01(float p)
    {
        if (isclose(p, 0f))
            return 0f;
        if (isclose(p, 1f))
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
        List<float> func_params = functionSolver(A0, A1, A2, A3, A4);

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
        List<float> func_1_params = functionSolver(A0, A1, A2);
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
        List<float> func_1_params_sined = functionSolver(C, B, A);
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
        List<float> func_1_params = functionSolver(A0, A1, 0f);
        var points = from func_1_param in func_1_params
                     select a0 + a1 * func_1_param;
        return points.ToList();
    }

    public static bool isclose(float a, float b)
    {
        return Mathf.Abs(a - b) < 1e-3;
    }

    public static bool isclose(Vector2 a, Vector2 b){
        return isclose((a - b).magnitude, 0f);
    }

    public delegate float Del(float x);

    /*sove function: f(x) = targetValue, f' = gradient*/
    public static float newTown(Del function, Del gradient, float targetValue, float startValue = 1f)
    {
        float ans = startValue;
        for(int i = 0; i != 20; ++i)
        {
            ans = ans - (function(ans) - targetValue) / gradient(ans);
            if (isclose(function(ans), targetValue)){
                break;
            }
        }
        return ans;
    }

    public static float approach(Del monoFunction, float targetValue, float startValue = 0f){
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

    public static float minArg(Del function, float startValue){
        float stepBase = 0.1f;
        int stepCnt = 0;

        while (stepBase > 1e-6 && stepCnt < 1e5)
        {
            float step = (function(startValue) < function(startValue + stepBase)) ? -stepBase : stepBase;

            while (function(startValue) > function(startValue + step))
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

    public static float mapLength = 1000f;
}