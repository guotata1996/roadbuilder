using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class Stats : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        int tris = 0;
        int vertices = 0;
        GameObject[] ob = FindObjectsOfType(typeof(GameObject)) as GameObject[];
        for (int i = 0; i != ob.Length; ++i){
            if (ob[i].GetComponent<MeshFilter>() != null){
                tris += ob[i].GetComponent<MeshFilter>().mesh.triangles.Length / 3;
                vertices += ob[i].GetComponent<MeshFilter>().mesh.vertexCount;
            }
        }
        GetComponent<Text>().text = "#Triangle = " + tris.ToString() + "\n#vertices = " + vertices.ToString();

    }
}
