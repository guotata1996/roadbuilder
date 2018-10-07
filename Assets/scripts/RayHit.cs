using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayHit : MonoBehaviour {

    public Vector2 hitpoint;

	// Use this for initialization
	void Start () {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
	
	// Update is called once per frame
	void Update () {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 hitpoint3 = hit.point;
            hitpoint = new Vector2(hitpoint3.x, hitpoint3.z);
        }
        if (Input.GetKey(KeyCode.S)){
            transform.Translate(0f, 0f, transform.position.y * 0.1f);
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(0f, 0f, -transform.position.y * 0.1f);
        }
        if (Input.GetKey(KeyCode.UpArrow)){
            transform.Translate(0f, transform.position.y * 0.1f, 0f);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Translate(0f, -transform.position.y * 0.1f, 0f);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.Translate(-transform.position.y * 0.1f, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            transform.Translate(transform.position.y * 0.1f, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.Q)){
            transform.Rotate(-2f, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.A)){
            transform.Rotate(2f, 0f, 0f);
        }
	}
}
