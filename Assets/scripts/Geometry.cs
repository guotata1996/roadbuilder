using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class Geometry {
    public static bool Parallel(Vector2 line1, Vector2 line2)
    {
        return (Algebra.isclose(line1.x, 0) && Algebra.isclose(line2.x, 0)) 
            || Algebra.isclose(line1, Vector2.zero) || Algebra.isclose(line2, Vector2.zero)||
                      Algebra.isclose(line1.y / line1.x, line2.y / line2.x); 
        //return Algebra.isclose(line1.x * line2.y, line1.y * line2.x);
    }

    public static List<Vector2> curveIntersect(Curve c1, Curve c2)
    {
        List<Vector2> specialcase = new List<Vector2>();
        List<Vector2> commoncase = new List<Vector2>();

        if (c1 is Bezeir)
        {
            if (c2 is Bezeir)
            {
                commoncase = (intersect(c1 as Bezeir, c2 as Bezeir));
            }
            else
            {
                if (c2 is Arc)
                {
                    commoncase = intersect(c1 as Bezeir, c2 as Arc);
                }
                else
                {
                    commoncase = intersect(c1 as Bezeir, c2 as Line);
                }
            }
        }
        else
        {
            if (c1 is Arc)
            {
                if (c2 is Bezeir)
                {
                    commoncase = intersect(c1 as Arc, c2 as Bezeir);
                }
                else
                {
                    if (c2 is Arc)
                    {
                        commoncase = intersect(c1 as Arc, c2 as Arc);
                    }
                    else
                    {
                        commoncase = intersect(c1 as Arc, c2 as Line);
                    }
                }
            }

            else
            {
                if (c2 is Bezeir)
                {
                    commoncase = intersect(c1 as Line, c2 as Bezeir);
                }
                else
                {
                    if (c2 is Arc)
                    {   
                        commoncase = intersect(c1 as Line, c2 as Arc);
                    }
                    else
                    {
                        commoncase =  intersect(c1 as Line, c2 as Line);
                    }
                }
            }
        }

        if (c1.contains_2d(c2.at_2d(0))){
            specialcase.Add(c2.at_2d(0));
        }
        if (c1.contains_2d(c2.at_2d(1))){
            specialcase.Add(c2.at_2d(1));
        }
        if (c2.contains_2d(c1.at_2d(0))){
            specialcase.Add(c1.at_2d(0));
        }
        if (c2.contains_2d(c1.at_2d(1))){
            specialcase.Add(c1.at_2d(1));
        }

        commoncase.AddRange(specialcase);
        commoncase = commoncase.Distinct(new Vector2Comparator()).ToList();

        return commoncase;

    }


    private static List<Vector2> intersect(Bezeir b1, Bezeir b2)
    {
        Vector2 A2 = b1.P0 - 2 * b1.P1 + b1.P2;
        Vector2 A1 = -2 * b1.P0 + 2 * b1.P1;
        Vector2 A0 = b1.P0;
        Vector2 B2 = b2.P0 - 2 * b2.P1 + b2.P2;
        Vector2 B1 = -2 * b2.P0 + 2 * b2.P1;
        Vector2 B0 = b2.P0;

        return filter(Algebra.parametricFunctionSolver(A0, A1, A2, B0, B1, B2), b1, b2);
    }

    private static List<Vector2> intersect(Bezeir b1, Arc b2)
    {
        Vector2 A2 = b1.P0 - 2 * b1.P1 + b1.P2;
        Vector2 A1 = -2 * b1.P0 + 2 * b1.P1;
        Vector2 A0 = b1.P0;
        List<Vector2> candidatePoints = Algebra.parametricFunctionSolver(A0, A1, A2, b2.center, b2.radius);
        return filter(candidatePoints, b1, b2);
    }

    private static List<Vector2> intersect(Arc b1, Bezeir b2)
    {
        return intersect(b2, b1);
    }

    private static List<Vector2> intersect(Bezeir b1, Line b2)
    {
        Vector2 A2 = b1.P0 - 2 * b1.P1 + b1.P2;
        Vector2 A1 = -2 * b1.P0 + 2 * b1.P1;
        Vector2 A0 = b1.P0;
        List<Vector2> candidatePoints = Algebra.parametricFunctionSolver(A0, A1, A2, b2.start, (b2.end - b2.start).normalized);

        return filter(candidatePoints, b1, b2);
    }

    private static List<Vector2> intersect(Line b1, Bezeir b2)
    {

        return intersect(b2, b1);
    }

    private static List<Vector2> intersect(Arc c1, Arc c2)
    {
        List<Vector2> candidatePoints = Algebra.parametricFunctionSolver(c1.center, c1.radius, c2.center, c2.radius);
        return filter(candidatePoints, c1, c2);
    }

    private static List<Vector2> intersect(Line b1, Arc b2)
    {
        List<Vector2> candidatePoints = Algebra.parametricFunctionSolver(b1.start, (b1.end - b1.start).normalized, b2.center, b2.radius);

        return filter(candidatePoints, b1, b2);
    }

    private static List<Vector2> intersect(Arc b1, Line b2)
    {
        return intersect(b2, b1);
    }

    private static List<Vector2> intersect(Line b1, Line b2)
    {
        List<Vector2> candiatePoints = Algebra.parametricFunctionSolver(b1.start, (b1.end - b1.start).normalized, b2.start, (b2.end - b2.start).normalized);
        return filter(candiatePoints, b1, b2);
    }

    private static List<Vector2> filter(List<Vector2> points, Curve c1, Curve c2)
    {
        var valids = 
        from point in points
        where c1.contains_2d(point) && c2.contains_2d(point)
        select point;

        return valids.ToList();
    }

    public static bool sameMotherCurveUponIntersect(Curve c1, Curve c2){
        if (c1 is Line)
        {
            if (c2 is Line)
            {
                return Parallel(((Line)c1).end - ((Line)c1).start, ((Line)c2).end - ((Line)c2).start);
            }
            else{
                return false;
            }
        }
        if (c1 is Arc){
            if (c2 is Arc){
                return Algebra.isclose(((((Arc)c1).center) - ((Arc)c2).center).magnitude, 0f);
            }
            else{
                return false;
            }
        }
        if (c1 is Bezeir){
            if (c2 is Bezeir){
                return Algebra.isclose(((((Bezeir)c1).P0) - ((Bezeir)c2).P0).magnitude, 0f)
                              && Algebra.isclose(((((Bezeir)c1).P1) - ((Bezeir)c2).P1).magnitude, 0f)
                              && Algebra.isclose(((((Bezeir)c1).P2) - ((Bezeir)c2).P2).magnitude, 0f);
            }
            return false;
        }
        return false;
    }

}

class Vector2Comparator : IEqualityComparer<Vector2>{
    public bool Equals(Vector2 x, Vector2 y)
{
    return Algebra.isclose((x - y).magnitude, 0f);
}

    public int GetHashCode(Vector2 obj)
    {
        return Algebra.approximate(obj).GetHashCode();
    }
}
