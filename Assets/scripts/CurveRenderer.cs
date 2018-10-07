using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class CurveRenderer : MonoBehaviour {

	public float maxAngleDiff = 0.01f;

	public void CreateMesh(Curve curve, float width, Texture curveTexture, float offset = 0f, float z_offset = 0f){
		GetComponent<MeshRenderer> ().material.mainTexture = curveTexture;
		GetComponent<MeshRenderer> ().material.mainTexture.wrapMode = TextureWrapMode.Repeat;

		float curveAngle = curve.length;
		int segmentCount = Mathf.CeilToInt(curveAngle / maxAngleDiff);

		Vector3 z_offset_vec = new Vector3 (0f, z_offset, 0f);
		segmentCount = Mathf.Max (segmentCount, 2);

		Vector3[] vertices = new Vector3[segmentCount+2];

		vertices [0] = curve.at (0f) + curve.rightNormal (0f) * (width / 2 + offset) + z_offset_vec;
		for (int i = 0; i != segmentCount; ++i) {
			Vector3 mid_i = curve.at(i * 1.0f / (segmentCount - 1)) + curve.rightNormal(i * 1.0f / (segmentCount - 1)) * offset;
			Vector3 norm_i = curve.rightNormal (i * 1.0f / (segmentCount - 1));
			vertices [i+1] = mid_i + norm_i * (width / 2) * Mathf.Sign (i % 2 - 0.5f) + z_offset_vec;
		}
		vertices [segmentCount + 1] = curve.at (1f) + curve.rightNormal(1f) * offset + curve.rightNormal (1f) * (width / 2) * Mathf.Sign (segmentCount % 2 - 0.5f) + z_offset_vec;

		int[] triangles = new int[segmentCount * 3];
		for (int i = 0; i != segmentCount; ++i) {
			Vector3 di_1 = vertices [i + 1] - vertices [i];
			Vector3 di_2 = vertices [i + 2] - vertices [i];
			if (Vector3.Cross(di_1, di_2).y > 0) {
				triangles [3 * i] = i;
				triangles [3 * i + 1] = i + 1;
				triangles [3 * i + 2] = i + 2;
			} else {
				triangles [3 * i] = i;
				triangles [3 * i + 2] = i + 1;
				triangles [3 * i + 1] = i + 2;
			}
		}

		Vector2[] uvs = new Vector2[segmentCount + 2];
		uvs [0] = new Vector2 (0f, 0f);
		for (int i = 0; i != segmentCount; ++i) {
			uvs [i + 1] = new Vector2 (0.5f * (Mathf.Sign (i % 2 - 0.5f) + 1), i * 1.0f / (segmentCount - 1));
		}
		uvs[segmentCount + 1] = new Vector2(0.5f * (Mathf.Sign(segmentCount % 2 - 0.5f) + 1) ,1f);

		Mesh mesh = new Mesh ();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;


		MeshFilter meshFilter = GetComponent<MeshFilter> ();
		meshFilter.mesh = mesh;

	}
}
