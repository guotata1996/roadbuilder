using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*an 2-D polygon*/
public class Polygon
{
    public Polygon(List<Curve> components)
    {
        Debug.Assert(components.TrueForAll((Curve obj) => obj.z_start == 0f && obj.z_offset == 0f));
        /*check if vertices form a closed loop*/

        for (int i = 0; i != components.Count - 1; ++i){
            Debug.Assert(Algebra.isclose(components[i].at_ending(false), components[i + 1].at_ending(true)));
        }
        Debug.Assert(Algebra.isclose(components[0].at_ending(true), components[components.Count - 1].at_ending(false)));

        this.components = components;
    }
    
    List<Curve> components; //should be in counterclockwise order

    float minresolution = 0.05f;
    int maxSegmentPerCurve = 15;

    public List<Curve> toFragments(){
        List<Curve> rtn = new List<Curve>();
        foreach(Curve curve in components){
            if (curve is Line)
            {
                rtn.Add(curve);
            }
            else
            {
                List<Curve> segs = curve.segmentation(maxlen: Mathf.Max(minresolution, curve.length / maxSegmentPerCurve));
                rtn.AddRange(segs);
            }
        }
        return rtn;
    }

    public int[] createMeshTriangle(){
        List<Vector2> vertices = toFragments().ConvertAll((Curve input) => input.at_ending_2d(true));
        List<Vector2> vertices_copy_unchanged = vertices.ConvertAll((input) => input);
        List<int> triangle = new List<int>();

        while (vertices.Count > 3){
            int earIndex = findEar(vertices);
            triangle.Add(vertices_copy_unchanged.IndexOf(vertices[earIndex]));
            triangle.Add(vertices_copy_unchanged.IndexOf(vertices[(earIndex + vertices.Count - 1) % vertices.Count]));
            triangle.Add(vertices_copy_unchanged.IndexOf(vertices[(earIndex + 1) % vertices.Count]));

            vertices.RemoveAt(earIndex);
        }
        triangle.Add(vertices_copy_unchanged.IndexOf(vertices[0]));
        triangle.Add(vertices_copy_unchanged.IndexOf(vertices[2]));
        triangle.Add(vertices_copy_unchanged.IndexOf(vertices[1]));

        return triangle.ToArray();
    }


    int findEar(List<Vector2> vertices){
        for (int i = 0; i != vertices.Count; ++i)
        {
            Vector2 previous = vertices[(i + vertices.Count - 1) % vertices.Count];
            Vector2 me = vertices[i];
            Vector2 following = vertices[(i + 1) % vertices.Count];
            if (Algebra.twodCross(following - me, previous - me) > 0)
            {
                bool not_affect_others = true;
                for (int index = (i + 2) % vertices.Count; index != (i + vertices.Count - 1) % vertices.Count; index = (index + 1) % vertices.Count){
                    if(Geometry.TriangleContains(previous, me, following, vertices[index])){
                        not_affect_others = false;
                        break;
                    }
                }
                if (not_affect_others)
                {
                    return i;
                }
            }
        }
        Debug.Assert(false);
        return -1;
    }

    public Vector2[] createUV(){
        List<Vector2> vertices = toFragments().ConvertAll((Curve input) => input.at_ending_2d(true));

        float LM = vertices.Min(c => c.x);
        float RM = vertices.Max(c => c.x);
        float BM = vertices.Min(c => c.y);
        float TM = vertices.Max(c => c.y);
        Debug.Assert(LM != RM);
        Debug.Assert(TM != BM);

        return vertices.ConvertAll((input) => new Vector2((input.x - LM) / (RM - LM), (input.y - BM) / (TM - BM))).ToArray();
    }
}
