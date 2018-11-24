using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/*an 2-D polygon*/
public class Polygon
{
    /*resized_z = original_z + v_resize_proportions * deltah */
    public Polygon(List<Curve> components, List<float> v_resize_proportions = null)
    {
        Debug.Assert(components.TrueForAll((Curve obj) => obj.z_start == 0f && obj.z_offset == 0f));
        /*check if vertices form a closed loop*/

        for (int i = 0; i != components.Count - 1; ++i){
            Debug.Assert(Algebra.isclose(components[i].at_ending(false), components[i + 1].at_ending(true)));
        }
        Debug.Assert(Algebra.isclose(components[0].at_ending(true), components[components.Count - 1].at_ending(false)));
        Debug.Assert(v_resize_proportions == null || v_resize_proportions.Count == components.Count);
        this.components = components;
        if (v_resize_proportions == null)
        {
            v_resize_proportions = new List<float>();
            for (int i = 0; i != components.Count; ++i){
                v_resize_proportions.Add(0f);
            }
        }
        createFragments(v_resize_proportions);
    }
    
    List<Curve> components; //should be in counterclockwise order
    /*calculated value*/
    List<float> v_resize_proportions;
    List<Vector2> fragments;

    float minresolution = 0.15f;

    int maxSegmentPerCurve = 15;

    void createFragments(List<float> v_resize_proportions_input)
    {
        fragments = new List<Vector2>();
        v_resize_proportions = new List<float>();
        foreach(Curve curve in components){
            if (curve is Line)
            {
                fragments.Add(curve.at_ending_2d(true));
                v_resize_proportions.Add(v_resize_proportions_input[this.components.IndexOf(curve)]);
            }
            else
            {
                List<Vector2> segs = curve.segmentation(maxlen: Mathf.Max(minresolution, curve.length / maxSegmentPerCurve)).ConvertAll((Curve input) => input.at_ending_2d(true));
                segs.RemoveAt(segs.Count - 1);

                int currentNode = components.IndexOf(curve);
                int nextNode = (currentNode + 1) % components.Count;
                List<float> offsetSegs = segs.ConvertAll((input) => (1f - (float)curve.paramOf(input)) * 
                    v_resize_proportions_input[currentNode] + (float)curve.paramOf(input) * v_resize_proportions_input[nextNode]);

                fragments.AddRange(segs);
                v_resize_proportions.AddRange(offsetSegs);
            }
        }
    }

    public List<Vector2> toFragments(){
        return fragments.ConvertAll((input) => input);
    }

    public List<float> getVResizeOffset(float h){
        return v_resize_proportions.ConvertAll((float input) => input * h);
    }


    public int[] createMeshTriangle(){
        List<Vector2> vertices = this.toFragments();


        //string pr = "";
        //foreach (Vector2 v in vertices){
        //    pr += v.x.ToString("F3");
        //    pr += ",";
        //    pr += v.y.ToString("F3");
        //    pr += "; ";
        //}
        //Debug.Log(pr);
        //Debug.Log("--------");


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
        List<Vector2> vertices = toFragments();

        float LM = vertices.Min(c => c.x);
        float RM = vertices.Max(c => c.x);
        float BM = vertices.Min(c => c.y);
        float TM = vertices.Max(c => c.y);
        Debug.Assert(LM != RM);
        Debug.Assert(TM != BM);
        return vertices.ConvertAll((input) => new Vector2((input.x - LM) / (RM - LM), (input.y - BM) / (TM - BM))).ToArray();
    }

    public float[] createSideUVY(){
        List<Vector2> vertices = toFragments();
        float[] rtn = new float[vertices.Count];
        rtn[0] = 0f;
        for (int i = 0; i != vertices.Count; ++i){
            int ihat = (i + 1) % vertices.Count;
            rtn[i] = i == 0 ? (vertices[ihat] - vertices[i]).magnitude : rtn[i - 1] + (vertices[ihat] - vertices[i]).magnitude;
        }
        Debug.Assert(rtn[vertices.Count - 1] > 0f);
        for (int i = 0; i != vertices.Count; ++i){
            rtn[i] /= rtn[vertices.Count - 1];
        }
        return rtn;
    }
}
