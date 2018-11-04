using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class CurveRenderer : MonoBehaviour
{

    float maxAngleDiff = 0.1f;

    public void CreateMesh(Curve curve, float width, Material mainMaterial, float offset = 0f, float z_offset = 0f)
    {
        Mesh mesh = CreateMesh(curve, mainMaterial, new Vector2(offset + width / 2, z_offset), new Vector2(offset - width / 2, z_offset));
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    public void CreateMesh(Curve curve, float offset, Material mainMaterial, Polygon cross)
    {
        List<Curve> fragments = cross.toFragments();
        Mesh mesh = CreateMesh(curve, mainMaterial, new Vector2(offset, 0f) + fragments[0].at_ending_2d(true), new Vector2(offset, 0f) + fragments[0].at_ending_2d(false));
        int[] crossTriangles = cross.createMeshTriangle();
        Vector2[] crossUVs = cross.createUV();
        int crossVerticeCount = fragments.Count;

        Vector3[] all_vertices = new Vector3[fragments.Count * mesh.vertices.Length + crossVerticeCount * 2];
        int[] all_triangles = new int[fragments.Count * mesh.triangles.Length + crossTriangles.Length * 2];
        Vector2[] all_uvs = new Vector2[fragments.Count * mesh.uv.Length + crossVerticeCount * 2];
        //Debug.Log(fragments.Count * mesh.vertices.Length);

        var watch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i != fragments.Count; ++i)
        {
            Mesh mesh1 = CreateMesh(curve, mainMaterial, new Vector2(offset, 0f) + fragments[i].at_ending_2d(true), new Vector2(offset, 0f) + fragments[i].at_ending_2d(false));
            for (int j = 0; j != mesh.vertices.Length; ++j){
                all_vertices[i * mesh.vertices.Length + j] = mesh1.vertices[j];
            }
            for (int j = 0; j != mesh.triangles.Length; ++j){
                all_triangles[i * mesh.triangles.Length + j] = mesh1.triangles[j] + i * mesh.vertices.Length;
            }
            for (int j = 0; j != mesh.uv.Length; ++j){
                all_uvs[i * mesh.uv.Length + j] = mesh1.uv[j];
            }
        }
        watch.Stop();

        print("create" + fragments.Count + "*" + mesh.vertices.Length + " mesh costs " + watch.ElapsedMilliseconds + "ms");


        /*Add mesh at start*/

        for (int j = 0; j != crossVerticeCount; ++j)
        {
            all_vertices[fragments.Count * mesh.vertices.Length + j] = curve.at_ending(true) +
                                                                            curve.rightNormal(0f) * (offset + fragments[j].at_ending_2d(true).x)
                                                                            + Vector3.up * fragments[j].at_ending_2d(true).y;
        }
        for (int j = 0; j != crossTriangles.Length; ++j){
            all_triangles[fragments.Count * mesh.triangles.Length + j] = crossTriangles[j] + fragments.Count * mesh.vertices.Length;
            //Debug.Log("triangle " + (fragments.Count * mesh.triangles.Length + j) + " set to " + (all_vertices[crossTriangles[j] + fragments.Count * mesh.vertices.Length]));
        }
        for (int j = 0; j != crossVerticeCount; ++j){
            all_uvs[fragments.Count * mesh.uv.Length + j] = crossUVs[j];
        }

        /*Add mesh at end*/
        for (int j = 0; j != crossVerticeCount; ++j)
        {
            all_vertices[fragments.Count * mesh.vertices.Length + crossVerticeCount + j] = curve.at_ending(false) +
                                                                                                curve.rightNormal(1f) * (offset + fragments[j].at_ending_2d(true).x)
                                                                            + Vector3.up * fragments[j].at_ending_2d(true).y;
        }

        for (int j = 0; j != crossTriangles.Length; ++j){
            if (j % 3 == 1)
            {
                all_triangles[fragments.Count * mesh.triangles.Length + crossTriangles.Length + j] = crossTriangles[j + 1];
            }
            else{
                if (j % 3 == 2){
                    all_triangles[fragments.Count * mesh.triangles.Length + crossTriangles.Length + j] = crossTriangles[j - 1];
                }
                else{
                    all_triangles[fragments.Count * mesh.triangles.Length + crossTriangles.Length + j] = crossTriangles[j];
                }
            }
            all_triangles[fragments.Count * mesh.triangles.Length + crossTriangles.Length + j] += fragments.Count * mesh.vertices.Length + crossVerticeCount;
        }
        for (int j = 0; j != crossVerticeCount; ++j)
        {
            all_uvs[fragments.Count * mesh.uv.Length + crossVerticeCount + j] = crossUVs[j];
        }


        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh total_mesh = new Mesh();
        total_mesh.vertices = all_vertices;
        total_mesh.triangles = all_triangles;
        total_mesh.uv = all_uvs;
        meshFilter.mesh = total_mesh;
    }

    public Mesh CreateMesh(Curve curve, Material mainMaterial, Vector2 offset1, Vector2 offset2)
    {
        GetComponent<MeshRenderer>().material = mainMaterial;

        float curveAngle = curve.length;
        int segmentCount = Mathf.CeilToInt(curveAngle / maxAngleDiff);

        segmentCount = Mathf.Max(segmentCount, 2);

        Vector3[] vertices = new Vector3[segmentCount + 2];

        vertices[0] = curve.at(0f) + curve.rightNormal(0f) * offset1.x + Vector3.up * offset1.y;
        for (int i = 0; i != segmentCount; ++i)
        {
            //Vector3 mid_i = curve.at(i * 1.0f / (segmentCount - 1)) + curve.rightNormal(i * 1.0f / (segmentCount - 1)) * offset;
            //Vector3 norm_i = curve.rightNormal(i * 1.0f / (segmentCount - 1));
            //vertices[i + 1] = mid_i + norm_i * (width / 2) * Mathf.Sign(i % 2 - 0.5f) + z_offset_vec;
            float curveParam = i * 1.0f / (segmentCount - 1);
            vertices[i + 1] = (i % 2 == 1) ? curve.at(curveParam) + curve.rightNormal(curveParam) * offset1.x + Vector3.up * offset1.y :
                                                  curve.at(curveParam) + curve.rightNormal(curveParam) * offset2.x + Vector3.up * offset2.y;
        }
        //vertices[segmentCount + 1] = curve.at(1f) + curve.rightNormal(1f) * offset + curve.rightNormal(1f) * (width / 2) * Mathf.Sign(segmentCount % 2 - 0.5f) + z_offset_vec;
        vertices[segmentCount + 1] = (segmentCount % 2 == 1) ? curve.at(1f) + curve.rightNormal(1f) * offset1.x + Vector3.up * offset1.y :
                                                                    curve.at(1f) + curve.rightNormal(1f) * offset2.x + Vector3.up * offset2.y;
        int[] triangles = new int[segmentCount * 3];
        for (int i = 0; i != segmentCount; ++i)
        {
            float curveParam = i * 1.0f / (segmentCount - 1);
            Vector3 projectionPlane = Vector3.Cross(Vector3.up, curve.rightNormal(curveParam)).normalized;

            Vector3 di_1 = vertices[i + 1] - vertices[i];
            Vector3 di_2 = vertices[i + 2] - vertices[i];
            Vector3 pieceNorm = Vector3.Cross(di_1, di_2);
            Vector3 projection = pieceNorm - projectionPlane * Vector3.Dot(pieceNorm, projectionPlane);
            Vector2 twod_projection = new Vector2(Vector2.Dot(projection, curve.rightNormal(curveParam)), Vector2.Dot(projection, Vector3.up));

            if (Algebra.twodCross(offset2 - offset1, twod_projection) <= 0)
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

        Vector2[] uvs = new Vector2[segmentCount + 2];
        uvs[0] = new Vector2(0f, 0f);
        for (int i = 0; i != segmentCount; ++i)
        {
            uvs[i + 1] = new Vector2(0.5f * (Mathf.Sign(i % 2 - 0.5f) + 1), i * 1.0f / (segmentCount - 1));
        }
        uvs[segmentCount + 1] = new Vector2(0.5f * (Mathf.Sign(segmentCount % 2 - 0.5f) + 1), 1f);

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        return mesh;

    }


}