using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayHit : MonoBehaviour {

    public Vector2 hitpoint;
    public float zoomStep = 0.02f;
    Vector3 hitpoint3;
    float MinimumPitch = 90f, MaximumPitch = 20f;
    float MaximumHeight = 110f;

    Vector3 targetPosition;   //intended position of camera right on top

	// Use this for initialization
	void Start () {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
        targetPosition = transform.position;
    }
	
	// Update is called once per frame
	void Update () {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            hitpoint3 = hit.point;
            hitpoint = new Vector2(hitpoint3.x, hitpoint3.z);
        }
        var scrollWheel = Input.GetAxis("Mouse ScrollWheel");

        if (!Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.GetKey(KeyCode.S) || scrollWheel > 0)
            {
                float speedMultipler = Mathf.Max(1f, scrollWheel * 5f);
                targetPosition.x = hitpoint3.x * zoomStep * speedMultipler + targetPosition.x * (1 - zoomStep * speedMultipler);
                targetPosition.y = -hitpoint3.y * 0.5f * zoomStep * speedMultipler + targetPosition.y * (1 + 0.5f * zoomStep * speedMultipler);
                targetPosition.z = hitpoint3.z * zoomStep * speedMultipler + targetPosition.z * (1 - zoomStep * speedMultipler);

                Vector3 original = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(new Vector3(Mathf.Max(MaximumPitch, original.x), original.y, original.z));

            }
            if (Input.GetKey(KeyCode.W) || scrollWheel < 0)
            {
                float speedMultipler = Mathf.Max(1f, -scrollWheel * 5f);

                targetPosition.x = hitpoint3.x * zoomStep * speedMultipler + targetPosition.x * (1 - zoomStep * speedMultipler);
                targetPosition.y = hitpoint3.y * 0.5f * zoomStep * speedMultipler + targetPosition.y * (1 - 0.5f * zoomStep * speedMultipler);
                targetPosition.z = hitpoint3.z * zoomStep * speedMultipler + targetPosition.z * (1 - zoomStep * speedMultipler);

                Vector3 original = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(new Vector3(Mathf.Min(MinimumPitch, original.x), original.y, original.z));

            }
            MinimumPitch = Mathf.Min(targetPosition.y * 1.5f + 40f, 90f);
            MaximumPitch = Mathf.Max(targetPosition.y * 0.5f + 15f, 20f);
            targetPosition.y = Mathf.Min(targetPosition.y, MaximumHeight);
        }
        else
        {
            if (Input.GetKey(KeyCode.S) || scrollWheel > 0)
            {
                float speedMultipler = Mathf.Max(1f, scrollWheel * 5f);
                Vector3 original = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(new Vector3(Mathf.Max(MaximumPitch, original.x - 0.4f * speedMultipler), original.y, original.z));
            }
            if (Input.GetKey(KeyCode.W) || scrollWheel < 0)
            {
                float speedMultipler = Mathf.Max(1f, -scrollWheel * 5f);
                Vector3 original = transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(new Vector3(Mathf.Min(MinimumPitch, original.x + 0.4f * speedMultipler), original.y, original.z));
            }
        }
        if (Input.GetKey(KeyCode.A))
        {
            Vector3 original = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(new Vector3(original.x, original.y - 0.4f, original.z));
        }
        if (Input.GetKey(KeyCode.D)){
            Vector3 original = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(new Vector3(original.x, original.y + 0.4f, original.z));
        }


        Vector2 xz_offset = -Mathf.Tan(Mathf.Deg2Rad * (transform.rotation.eulerAngles.x - 90f)) * targetPosition.y *
                                  Algebra.angle2dir(Mathf.Deg2Rad * (270f - transform.rotation.eulerAngles.y));
        Vector3 biasedPosition = targetPosition + new Vector3(xz_offset.x, 0f, xz_offset.y);
        transform.position = Vector3.Lerp(transform.position, biasedPosition, Time.deltaTime * 5);
    }
}
