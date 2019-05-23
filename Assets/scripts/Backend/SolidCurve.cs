using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class SolidCurve
{
    public static GameObject Generate(IEnumerator line, IEnumerable sector)
    {
        var line_pos_samples = new List<Vector3>();
        var line_up_samples = new List<Vector3>();
        var line_right_samples = new List<Vector3>();

        line.Reset();   //Reset must be manually called
        while (line.MoveNext())
        {
            Vector3 pos, right, front;
            (pos, right, front) = ((Vector3, Vector3, Vector3))line.Current;
            line_pos_samples.Add(pos);
            line_right_samples.Add(right);
            line_up_samples.Add(Vector3.Cross(right, front));
        }
        
        List<Vector2> sector_vertices = new List<Vector2>();
        
        foreach(Vector2 p in sector)
        {
            sector_vertices.Add(p);
        }

        Debug.Log("#Line= " + line_pos_samples.Count + " #SecV= " + sector_vertices.Count);

        int line_seg_count = line_pos_samples.Count - 1;
        int base_vcount = (line_seg_count + 1) * sector_vertices.Count;
        int base_tcount = line_seg_count * 2 * 3 * sector_vertices.Count;

        Vector3[] linear_vertices = new Vector3[base_vcount];
        int[] linear_triangles = new int[base_tcount];

        for (int i = 0; i != line_seg_count + 1; ++i)
        {
            List<Vector3> local_vertices = sector_vertices.ConvertAll(
            (input) => line_pos_samples[i] 
                + input.x * line_right_samples[i]
                + input.y * line_up_samples[i]);

            for (int j = i * sector_vertices.Count, local_j = 0; j != i * sector_vertices.Count + sector_vertices.Count; ++j, ++local_j)
            {
                linear_vertices[j] = local_vertices[local_j];
            }
        }

        for (int i = 0, triangle = 0; i < line_seg_count; ++i)
        {
            for (int j = 0; j != sector_vertices.Count; ++j, triangle += 6)
            {
                linear_triangles[triangle] = i * sector_vertices.Count + j;
                linear_triangles[triangle + 1] = i * sector_vertices.Count + (j + 1) % sector_vertices.Count;
                linear_triangles[triangle + 2] = (i + 1) * sector_vertices.Count + j;

                linear_triangles[triangle + 3] = i * sector_vertices.Count + (j + 1) % sector_vertices.Count;
                linear_triangles[triangle + 4] = (i + 1) * sector_vertices.Count + (j + 1) % sector_vertices.Count;
                linear_triangles[triangle + 5] = (i + 1) * sector_vertices.Count + j;
            }

        }

        GameObject gameObject = new GameObject("SolidCurve", typeof(MeshFilter), typeof(MeshRenderer));
        gameObject.GetComponent<MeshFilter>().sharedMesh = new Mesh();
        gameObject.GetComponent<MeshFilter>().sharedMesh.SetVertices(linear_vertices.ToList());
        gameObject.GetComponent<MeshFilter>().sharedMesh.SetTriangles(linear_triangles.ToList(), 0);

        return gameObject;
    }
}
