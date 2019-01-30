using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;
public enum angleType { Sharp, Blunt, Flat, Reflex};

public static class Geometry {

    public static bool Parallel(Vector2 line1, Vector2 line2)
    {
        return (Algebra.isclose(line1.x, 0) && Algebra.isclose(line2.x, 0)) 
            || Algebra.isclose(line1, Vector2.zero) || Algebra.isclose(line2, Vector2.zero)||
                      Algebra.isclose(line1.y / line1.x, line2.y / line2.x); 
        //return Algebra.isclose(line1.x * line2.y, line1.y * line2.x);
    }

    public static bool Parallel(Vector3 line1, Vector3 line2)
    {
        return Parallel(new Vector2(line1.x, line1.z), new Vector2(line2.x, line2.z));
    }

    /*if flattened, return Point on c1*/
    public static List<Vector3> curveIntersect(Curve c1, Curve c2)
    {
        List<Vector3> specialcase = new List<Vector3>();
        List<Vector3> commoncase = new List<Vector3>();

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

        if (c1.contains(c2.at(0))){
            specialcase.Add(c2.at(0));
        }
        if (c1.contains(c2.at(1))){
            specialcase.Add(c2.at(1));
        }
        if (c2.contains(c1.at(0))){
            specialcase.Add(c1.at(0));
        }
        if (c2.contains(c1.at(1))){
            specialcase.Add(c1.at(1));
        }

        commoncase.AddRange(specialcase);
        commoncase = commoncase.Distinct(new IntersectPointComparator()).ToList();
        return commoncase;

    }

    internal static bool TriangleContains(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        Vector3 v0 = C - A;
        Vector3 v1 = B - A;
        Vector3 v2 = P - A;

        double dot00 = Vector2.Dot(v0, v0);
        double dot01 = Vector2.Dot(v0, v1);
        double dot02 = Vector2.Dot(v0, v2);
        double dot11 = Vector2.Dot(v1, v1);
        double dot12 = Vector2.Dot(v1, v2);

        double inverDeno = 1 / (dot00 * dot11 - dot01 * dot01);

        double u = (dot11 * dot02 - dot01 * dot12) * inverDeno;
        if (u < 0 && !Algebra.isclose(u, 0)|| u > 1 && !Algebra.isclose(u, 1)) // if u out of range, return directly
        {
            return false;
        }

        double v = (dot00 * dot12 - dot01 * dot02) * inverDeno;
        if (v < 0 && !Algebra.isclose(v, 0)|| v > 1 && !Algebra.isclose(v, 1)) // if v out of range, return directly
        {
            return false;
        }

        return u + v <= 1 || Algebra.isclose(u+v, 1);
    }

    private static List<Vector3> intersect(Bezeir b1, Bezeir b2)
    {
        Vector2 A2 = b1.P0 - 2 * b1.P1 + b1.P2;
        Vector2 A1 = -2 * b1.P0 + 2 * b1.P1;
        Vector2 A0 = b1.P0;
        Vector2 B2 = b2.P0 - 2 * b2.P1 + b2.P2;
        Vector2 B1 = -2 * b2.P0 + 2 * b2.P1;
        Vector2 B0 = b2.P0;

        return filter(Algebra.parametricFunctionSolver(A0, A1, A2, B0, B1, B2), b1, b2);
    }

    private static List<Vector3> intersect(Bezeir b1, Arc b2)
    {
        Vector2 A2 = b1.P0 - 2 * b1.P1 + b1.P2;
        Vector2 A1 = -2 * b1.P0 + 2 * b1.P1;
        Vector2 A0 = b1.P0;
        List<Vector2> candidatePoints = Algebra.parametricFunctionSolver(A0, A1, A2, b2.center, b2.radius);
        return filter(candidatePoints, b1, b2);
    }

    private static List<Vector3> intersect(Arc b1, Bezeir b2)
    {
        return intersect(b2, b1);
    }

    private static List<Vector3> intersect(Bezeir b1, Line b2)
    {
        Vector2 A2 = b1.P0 - 2 * b1.P1 + b1.P2;
        Vector2 A1 = -2 * b1.P0 + 2 * b1.P1;
        Vector2 A0 = b1.P0;
        List<Vector2> candidatePoints = Algebra.parametricFunctionSolver(A0, A1, A2, b2.start, (b2.end - b2.start).normalized);

        return filter(candidatePoints, b1, b2);
    }

    private static List<Vector3> intersect(Line b1, Bezeir b2)
    {

        return intersect(b2, b1);
    }

    private static List<Vector3> intersect(Arc c1, Arc c2)
    {
        List<Vector2> candidatePoints;
        if (sameMotherCurveUponIntersect(c1, c2))
        {
            candidatePoints = new List<Vector2>();
            if (Algebra.isclose(c1.at_ending(true), c2.at_ending(true)))
            {
                candidatePoints.Add(c1.at_ending_2d(true));
            }
            if (Algebra.isclose(c1.at_ending(true), c2.at_ending(false)))
            {
                candidatePoints.Add(c1.at_ending_2d(true));
            }
            if (Algebra.isclose(c1.at_ending(false), c2.at_ending(true)))
            {
                candidatePoints.Add(c1.at_ending_2d(false));
            }
            if (Algebra.isclose(c1.at_ending(false), c2.at_ending(false)))
            {
                candidatePoints.Add(c1.at_ending_2d(false));
            }
        }
        else
        {
            candidatePoints = Algebra.parametricFunctionSolver(c1.center, c1.radius, c2.center, c2.radius);
        }
        return filter(candidatePoints, c1, c2);
    }

    private static List<Vector3> intersect(Line b1, Arc b2)
    {
        List<Vector2> candidatePoints = Algebra.parametricFunctionSolver(b1.start, (b1.end - b1.start).normalized, b2.center, b2.radius);

        return filter(candidatePoints, b1, b2);
    }

    private static List<Vector3> intersect(Arc b1, Line b2)
    {
        return intersect(b2, b1);
    }

    private static List<Vector3> intersect(Line b1, Line b2)
    {
        List<Vector2> candiatePoints = Algebra.parametricFunctionSolver(b1.start, (b1.end - b1.start).normalized, b2.start, (b2.end - b2.start).normalized);
        return filter(candiatePoints, b1, b2);
    }

    private static List<Vector3> filter(List<Vector2> points, Curve c1, Curve c2)
    {
        var valids =
                from point in points
                where c2.contains_2d(point) && c1.contains(c2.at((float)c2.paramOf(point)))
                select new Vector3(point.x, c2.at((float)c2.paramOf(point)).y, point.y);

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

    public static angleType getAngleType(float angle){
        if (angle <= 0f || angle >= Mathf.PI * 2){
            Debug.LogError("invalid angle:" + angle);
        }
        if (angle <= Mathf.PI/2){
            return angleType.Sharp;
        }
        if (Algebra.isclose(angle, Mathf.PI)){
            return angleType.Flat;
        }

        if (Math.PI / 2 < angle && angle < Math.PI){
            return angleType.Blunt;
        }
        return angleType.Reflex;
    }
}

class IntersectPointComparator : IEqualityComparer<Vector3>{
    public bool Equals(Vector3 x, Vector3 y)
{
    return (x - y).magnitude < 0.1f;
}

    public int GetHashCode(Vector3 obj)
    {
        return 0;
    }
}
