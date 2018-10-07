using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class curveTypePanelBehavior : MonoBehaviour,
IPointerEnterHandler, IPointerExitHandler{

    public float min_visible_length;
    public float speed;
    private bool showingup;

	// Use this for initialization
	void Start () {
        showingup = false;
        this.transform.Translate(0f, min_visible_length - this.transform.GetComponent<RectTransform>().rect.height / 2 - transform.position.y, 0f);
	}
	
	// Update is called once per frame
	void Update () {
        float target_y = showingup ? this.transform.GetComponent<RectTransform>().rect.height / 2 : 
                                         min_visible_length - this.transform.GetComponent<RectTransform>().rect.height / 2;
        this.transform.Translate(0f, speed * (target_y - this.transform.position.y) * Time.deltaTime, 0f);
	}

    public void OnPointerEnter(PointerEventData eventData){
        this.showingup = true;
    }
    public void OnPointerExit(PointerEventData eventData){
        this.showingup = false;
    }
}
