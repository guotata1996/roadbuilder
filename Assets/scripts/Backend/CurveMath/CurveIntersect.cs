using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public abstract partial class Curve : ITwodPosAvailable
{
    public List<Vector2> IntersectWith(Curve another, bool filter_self = true, bool filter_other = true){
        Curve c1 = this;
        Curve c2 = another;
        
        List<Vector2> specialcase = new List<Vector2>();
        List<Vector2> commoncase = new List<Vector2>();

        if (c1 is Bezier)
        {
            if (c2 is Bezier)
            {
                commoncase = intersect(c1 as Bezier, c2 as Bezier, filter_self, filter_other);
            }
            else
            {
                if (c2 is Arc)
                {
                    commoncase = intersect(c1 as Bezier, c2 as Arc, filter_self, filter_other);
                }
                else
                {
                    commoncase = intersect(c1 as Bezier, c2 as Line, filter_self, filter_other);
                }
            }
        }
        else
        {
            if (c1 is Arc)
            {
                if (c2 is Bezier)
                {
                    commoncase = intersect(c1 as Arc, c2 as Bezier, filter_self, filter_other);
                }
                else
                {
                    if (c2 is Arc)
                    {
                        commoncase = intersect(c1 as Arc, c2 as Arc, filter_self, filter_other);
                    }
                    else
                    {
                        commoncase = intersect(c1 as Arc, c2 as Line, filter_self, filter_other);
                    }
                }
            }

            else
            {
                if (c2 is Bezier)
                {
                    commoncase = intersect(c1 as Line, c2 as Bezier, filter_self, filter_other);
                }
                else
                {
                    if (c2 is Arc)
                    {   
                        commoncase = intersect(c1 as Line, c2 as Arc, filter_self, filter_other);
                    }
                    else
                    {
                        commoncase =  intersect(c1 as Line, c2 as Line, filter_self, filter_other);
                    }
                }
            }
        }

        if (c1.Contains(c2.GetTwodPos(0))){
            specialcase.Add(c2.GetTwodPos(0));
        }
        if (c1.Contains(c2.GetTwodPos(1))){
            specialcase.Add(c2.GetTwodPos(1));
        }
        if (c2.Contains(c1.GetTwodPos(0))){
            specialcase.Add(c1.GetTwodPos(0));
        }
        if (c2.Contains(c1.GetTwodPos(1))){
            specialcase.Add(c1.GetTwodPos(1));
        }

        commoncase.AddRange(specialcase);
        commoncase = commoncase.Distinct(new IntersectPointComparator()).ToList();
        return commoncase;
    }

        private static List<Vector2> intersect(Bezier b1, Bezier b2, bool filter_self, bool filter_other)
        {
            Vector2 A2 = b1.P0 - 2 * b1.P1 + b1.P2;
            Vector2 A1 = -2 * b1.P0 + 2 * b1.P1;
            Vector2 A0 = b1.P0;
            Vector2 B2 = b2.P0 - 2 * b2.P1 + b2.P2;
            Vector2 B1 = -2 * b2.P0 + 2 * b2.P1;
            Vector2 B0 = b2.P0;

            return filter(Algebra.parametricFunctionSolver(A0, A1, A2, B0, B1, B2), b1, b2, filter_self, filter_other);
        }

        private static List<Vector2> intersect(Bezier b1, Arc b2, bool filter_self, bool filter_other)
        {
            Vector2 A2 = b1.P0 - 2 * b1.P1 + b1.P2;
            Vector2 A1 = -2 * b1.P0 + 2 * b1.P1;
            Vector2 A0 = b1.P0;
            List<Vector2> candidatePoints = Algebra.parametricFunctionSolver(A0, A1, A2, b2.Center, b2.Radius);
            return filter(candidatePoints, b1, b2, filter_self, filter_other);
        }

        private static List<Vector2> intersect(Arc b1, Bezier b2, bool filter_self, bool filter_other)
        {
            return intersect(b2, b1, filter_other, filter_self);
        }

        private static List<Vector2> intersect(Bezier b1, Line b2, bool filter_self, bool filter_other)
        {
            Vector2 A2 = b1.P0 - 2 * b1.P1 + b1.P2;
            Vector2 A1 = -2 * b1.P0 + 2 * b1.P1;
            Vector2 A0 = b1.P0;
            List<Vector2> candidatePoints = Algebra.parametricFunctionSolver(A0, A1, A2, b2.Start, (b2.End - b2.Start).normalized);

            return filter(candidatePoints, b1, b2, filter_self, filter_other);
        }

        private static List<Vector2> intersect(Line b1, Bezier b2, bool filter_self, bool filter_other)
        {

            return intersect(b2, b1, filter_other, filter_self);
        }

        private static List<Vector2> intersect(Arc c1, Arc c2, bool filter_self, bool filter_other)
        {
            List<Vector2> candidatePoints;
            if (sameMotherCurveUponIntersect(c1, c2))
            {
                candidatePoints = new List<Vector2>();
                if (Algebra.isclose(c1.GetTwodPos(0), c2.GetTwodPos(0)))
                {
                    candidatePoints.Add(c1.GetTwodPos(0));
                }
                if (Algebra.isclose(c1.GetTwodPos(0), c2.GetTwodPos(1)))
                {
                    candidatePoints.Add(c1.GetTwodPos(0));
                }
                if (Algebra.isclose(c1.GetTwodPos(1), c2.GetTwodPos(0)))
                {
                    candidatePoints.Add(c1.GetTwodPos(1));
                }
                if (Algebra.isclose(c1.GetTwodPos(1), c2.GetTwodPos(1)))
                {
                    candidatePoints.Add(c1.GetTwodPos(1));
                }
            }
            else
            {
                candidatePoints = Algebra.parametricFunctionSolver(c1.Center, c1.Radius, c2.Center, c2.Radius);
            }
            return filter(candidatePoints, c1, c2, filter_self, filter_other);
        }

        private static List<Vector2> intersect(Line b1, Arc b2, bool filter_self, bool filter_other)
        {
            List<Vector2> candidatePoints = Algebra.parametricFunctionSolver(b1.Start, (b1.End - b1.Start).normalized, b2.Center, b2.Radius);

            return filter(candidatePoints, b1, b2, filter_self, filter_other);
        }

        private static List<Vector2> intersect(Arc b1, Line b2, bool filter_self, bool filter_other)
        {
            return intersect(b2, b1, filter_other, filter_self);
        }

        private static List<Vector2> intersect(Line b1, Line b2, bool filter_self, bool filter_other)
        {
            List<Vector2> candiatePoints = Algebra.parametricFunctionSolver(b1.Start, (b1.End - b1.Start).normalized, b2.Start, (b2.End - b2.Start).normalized);
            return filter(candiatePoints, b1, b2, filter_self, filter_other);
        }

        private static List<Vector2> filter(List<Vector2> points, Curve c1, Curve c2, bool filter_self, bool filter_other)
        {

        //var valids =
        //from point in points
        //where c2.Contains(point) && c1.Contains(point)
        //select point;
            points = points.FindAll(input => c1.ParamOf(input) != null && c2.ParamOf(input) != null);

            if (filter_self)
            {
                points = points.FindAll(c1.Contains);
            }
            if (filter_other)
            {
                points = points.FindAll(c2.Contains);
            }
            return points.ToList();
        }

        
        static bool sameMotherCurveUponIntersect(Curve c1, Curve c2)
        {
            if (c1 is Line)
            {
                if (c2 is Line)
                {
                    return Algebra.Parallel(((Line)c1).End - ((Line)c1).Start, ((Line)c2).End - ((Line)c2).Start);
                }
                else{
                    return false;
                }
            }
            if (c1 is Arc){
                if (c2 is Arc){
                    return Algebra.isclose(((((Arc)c1).Center) - ((Arc)c2).Center).magnitude, 0f);
                }
                else{
                    return false;
                }
            }
            if (c1 is Bezier){
                if (c2 is Bezier){
                    return Algebra.isclose(((Bezier)c1).P0, ((Bezier)c2).P0)
                                && Algebra.isclose(((Bezier)c1).P1, ((Bezier)c2).P1)
                                && Algebra.isclose(((Bezier)c1).P2, ((Bezier)c2).P2);
                }
                return false;
            }
            return false;
        }

    class IntersectPointComparator : IEqualityComparer<Vector2>{
        public bool Equals(Vector2 x, Vector2 y)
    {
        return (x - y).magnitude < 0.1f;
    }

        public int GetHashCode(Vector2 obj)
        {
            return 0;
        }
    }
}
