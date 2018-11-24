using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class CurveRenderer : MonoBehaviour
{

    float maxAngleDiff = 0.1f;

    /*create mesh for flat surface*/
    public void CreateMesh(Curve curve, float width, Material mainMaterial, float offset = 0f, float z_offset = 0f)
    {
        Mesh mesh = CreateMesh(curve, mainMaterial, new Vector2(offset + width / 2, z_offset), new Vector2(offset - width / 2, z_offset));
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    /*create meash for linear rendered 3D object*/
    public void CreateMesh(Curve curve, float offset, Material linearMaterial, Material crossMaterial, Polygon cross)
    {
        List<Vector2> fragments = cross.toFragments();

        float curveAngle = curve.length;
        int segmentCount = Mathf.CeilToInt(curveAngle / maxAngleDiff);
        int base_vcount = (segmentCount + 1) * fragments.Count;
        int base_tcount = segmentCount * 2 * 3 * fragments.Count;

        int[] crossTriangles = cross.createMeshTriangle();
        int cross_vcount = fragments.Count;
        int cross_tcount = crossTriangles.Length;
        Vector2[] crossUVs = cross.createUV();

        Vector3[] all_vertices = new Vector3[base_vcount + 2*cross_vcount];
        int[] linear_triangles = new int[base_tcount];
        Vector2[] linearUVs = new Vector2[base_vcount];

        for (int i = 0; i != segmentCount + 1; ++i)
        {
            Vector3 roadPoint = curve.at(1.0f / segmentCount * i);
            float direction = curve.angle_2d(1.0f / segmentCount * i) - Mathf.PI / 2;
            List<Vector3> localFragments = fragments.ConvertAll((input) => roadPoint +
                                                                Algebra.toVector3(Algebra.twodRotate(Vector2.right * (offset + input.x), direction)) +
                                                                Vector3.up * input.y);
            /*stretch Z*/
            List<float> cross_y_offset = cross.getVResizeOffset(roadPoint.y);
            for (int j = 0; j != localFragments.Count; ++j)
            {
                localFragments[j] += Vector3.up * cross_y_offset[j];
            }

            float cross_diameter = fragments.Sum((input) => input.magnitude);
            float partial_diameter = 0f;
            for (int j = i * cross_vcount, local_j = 0; j != i * cross_vcount + cross_vcount; ++j, ++local_j)
            {
                all_vertices[j] = localFragments[local_j];
                linearUVs[j] = new Vector2(partial_diameter / cross_diameter, i * 1.0f / segmentCount);
                partial_diameter += fragments[local_j].magnitude;
            }
        }

        for (int i = 0; i != cross_vcount; ++i){
            all_vertices[base_vcount + i] = all_vertices[i];
            all_vertices[base_vcount + cross_vcount + i] = all_vertices[base_vcount - cross_vcount + i];
        }

        for (int i = 0, triangle = 0; i != segmentCount; ++i){
            for (int j = 0; j != cross_vcount; ++j, triangle += 6)
            {
                linear_triangles[triangle] = i * cross_vcount + j;
                linear_triangles[triangle + 1] = i * cross_vcount + (j + 1) % cross_vcount;
                linear_triangles[triangle + 2] = (i + 1) * cross_vcount + j;

                linear_triangles[triangle + 3] = i * cross_vcount + (j + 1) % cross_vcount;
                linear_triangles[triangle + 4] = (i + 1) * cross_vcount + (j + 1) % cross_vcount;
                linear_triangles[triangle + 5] = (i + 1) * cross_vcount + j;
            }

        }

        int[] cross_triangles = new int[2 * cross_tcount];
        /*Add tris at start*/
        for (int j = 0; j != cross_tcount; ++j){
            cross_triangles[j] = crossTriangles[j] + base_vcount;
        }

        /*Add tris at end*/
        for (int j = 0; j != cross_tcount; ++j){
            if (j % 3 == 1)
            {
                cross_triangles[cross_tcount + j] = crossTriangles[j + 1];
            }
            else{
                if (j % 3 == 2){
                    cross_triangles[cross_tcount + j] = crossTriangles[j - 1];
                }
                else{
                    cross_triangles[cross_tcount + j] = crossTriangles[j];
                }
            }
            cross_triangles[cross_tcount + j] += (base_vcount + cross_vcount);
        }

        Vector2[] modifiedCrossUVs = new Vector2[base_vcount + 2 * cross_vcount];
        for (int i = 0; i != base_vcount ; ++i)
        {
            modifiedCrossUVs[i] = linearUVs[i];
        }
        for (int i = 0; i != cross_vcount; ++i){
            modifiedCrossUVs[i + base_vcount] = modifiedCrossUVs[i + base_vcount + cross_vcount] = crossUVs[i];
        }


        GetComponent<MeshRenderer>().materials = new Material[2]{ crossMaterial, linearMaterial };

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh total_mesh = new Mesh();
        total_mesh.subMeshCount = 2;
        total_mesh.SetVertices(all_vertices.ToList());
        total_mesh.SetTriangles(cross_triangles, 0);
        total_mesh.SetTriangles(linear_triangles, 1);
        total_mesh.SetUVs(0, modifiedCrossUVs.ToList());
        //total_mesh.SetUVs(1, linearUVs.ToList());
        meshFilter.mesh = total_mesh;
        
    }

    Mesh CreateMesh(Curve curve, Material mainMaterial, Vector2 offset1, Vector2 offset2)
    {
        GetComponent<MeshRenderer>().material = mainMaterial;

        float curveAngle = curve.length;
        int segmentCount = Mathf.CeilToInt(curveAngle / maxAngleDiff);

        segmentCount = Mathf.Max(segmentCount, 2);

        Vector3[] vertices = new Vector3[segmentCount + 2];
        Vector2[] uvs = new Vector2[segmentCount + 2];


        for (int i = -1; i <= segmentCount; ++i)
        {
            float curveParam = Mathf.Clamp(i * 1.0f / (segmentCount - 1), 0f, 1f);
            vertices[i + 1] = (i == - 1 || i % 2 == 1) ? curve.at(curveParam) + curve.rightNormal(curveParam) * offset1.x + Vector3.up * offset1.y :
                                                  curve.at(curveParam) + curve.rightNormal(curveParam) * offset2.x + Vector3.up * offset2.y;
            uvs[i + 1] = (i % 2 == 1) ? new Vector2(0f, curveParam * curve.length / (offset1.x - offset2.x)) :
                new Vector2(1f, curveParam * curve.length / (offset1.x - offset2.x));
        }

        int[] triangles = new int[segmentCount * 3];
        for (int i = 0; i != segmentCount; ++i)
        {
            if (i % 2 == 0)
            {
                triangles[3 * i] = i;
                triangles[3 * i + 1] = i + 1;
                triangles[3 * i + 2] = i + 2;
            }
            else
            {
                triangles[3 * i] = i;
                triangles[3 * i + 2] = i + 1;
                triangles[3 * i + 1] = i + 2;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        return mesh;

    }

    /*TODO: Choose configuration*/
    public void CreateMesh(Polygon p, float h, Material mainMaterial){
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Vector2[] polyVertices = p.toFragments().ToArray();
        Vector3[] allVertices = new Vector3[4 * polyVertices.Length];
        for (int i = 0; i != polyVertices.Length; ++i){
            allVertices[i].x = allVertices[i + 2 * polyVertices.Length].x = polyVertices[i].x;
            allVertices[i].y = allVertices[i + 2 * polyVertices.Length].y = h;
            allVertices[i].z = allVertices[i + 2 * polyVertices.Length].z = polyVertices[i].y;
        }
        for (int i = 0; i != polyVertices.Length; ++i){
            allVertices[i + polyVertices.Length].x = allVertices[i + 3 * polyVertices.Length].x = polyVertices[i].x;
            allVertices[i + polyVertices.Length].y = allVertices[i + 3 * polyVertices.Length].y = h - 0.2f;
            allVertices[i + polyVertices.Length].z = allVertices[i + 3 * polyVertices.Length].z = polyVertices[i].y;
        }

        Vector2[] allUVs = new Vector2[4 * polyVertices.Length];
        Vector2[] polySurfaceUVs = p.createUV();
        float[] polySideUVYs = p.createSideUVY();
        for (int i = 0; i != polyVertices.Length; ++i){
            allUVs[i] = allUVs[i + polyVertices.Length] = polySurfaceUVs[i];
        }
        for (int i = 0; i != polyVertices.Length; ++i){
            allUVs[i + 2 * polyVertices.Length] = new Vector2(0, polySideUVYs[i]);
            allUVs[i + 3 * polyVertices.Length] = new Vector2(1, polySideUVYs[i]);
        }

        int[] polyTriangles = p.createMeshTriangle();
        int[] allTriangles = new int[polyTriangles.Length * 2 + 3 * polyVertices.Length * 2];

        for (int i = 0; i != polyTriangles.Length; ++i){
            allTriangles[i] = polyTriangles[i];
            if (i % 3 == 0){
                allTriangles[polyTriangles.Length + i] = polyTriangles[i] + polyVertices.Length;
            }
            if (i % 3 == 1){
                allTriangles[polyTriangles.Length + i] = polyTriangles[i+1] + polyVertices.Length;
            }
            if (i % 3 == 2)
            {
                allTriangles[polyTriangles.Length + i] = polyTriangles[i-1] + polyVertices.Length;
            }
        }

        for (int i = 0; i != polyVertices.Length; ++i){
            int nexti = (i + 1) % polyVertices.Length;
            allTriangles[2 * polyTriangles.Length + 3 * i] = i + 2 * polyVertices.Length;
            allTriangles[2 * polyTriangles.Length + 3 * i + 1] = nexti + 2 * polyVertices.Length;
            allTriangles[2 * polyTriangles.Length + 3 * i + 2] = i + (2+1) * polyVertices.Length;

            allTriangles[2 * polyTriangles.Length + 3 * polyVertices.Length + 3 * i] = i + (2+1) * polyVertices.Length;
            allTriangles[2 * polyTriangles.Length + 3 * polyVertices.Length + 3 * i + 1] = nexti + 2 * polyVertices.Length;
            allTriangles[2 * polyTriangles.Length + 3 * polyVertices.Length + 3 * i + 2] = nexti + (2 + 1) * polyVertices.Length;
        }

        GetComponent<MeshRenderer>().material = mainMaterial;
        Mesh mesh = new Mesh();
        mesh.vertices = allVertices;
        mesh.uv = allUVs;
        mesh.triangles = allTriangles;
        meshFilter.mesh = mesh;
    }
}