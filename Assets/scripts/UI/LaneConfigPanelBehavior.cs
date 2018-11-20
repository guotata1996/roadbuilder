using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;
using System;

public class LaneConfigPanelBehavior : MonoBehaviour
, IPointerEnterHandler, IPointerExitHandler{

    public float min_visible_length;
    public float speed;
    public float roll_delay_sec;

    public Button addButtonPrefab;

    public GameObject indicatorInstance;

    public List<string> laneconfigresult;

    List<Button> itemInstances = new List<Button>();

    // Use this for initialization
	void Start () {
        addNewChoice();
        this.transform.Translate(0f, min_visible_length - this.length/2 - transform.position.y, 0f);
	}
	

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(rollOrUnroll(true, this.speed));
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(rollOrUnroll(false, this.speed));
    }

    float length
    {
        get
        {
            return min_visible_length + itemInstances.Sum(b => b.GetComponent<RectTransform>().rect.height);
        }
    }

    float lengthToItem(int n){
        return itemInstances.GetRange(0, n).Sum(b => b.GetComponent<RectTransform>().rect.height);
    }

    void addNewChoice(){
        /*
        Button newchoice = Instantiate(addButtonPrefab,
                                       new Vector3(transform.position.x, this.length - min_visible_length
                                                   + addButtonPrefab.GetComponent<RectTransform>().rect.height / 2
                                                   , 0f), 
                                       Quaternion.identity);
                                       */
        Button newchoice = Instantiate(addButtonPrefab, transform);
        newchoice.transform.SetParent(transform);
        itemInstances.Add(newchoice);
        updatePanelDisplay();

    }

    void deleteChoice(int index){
        Destroy(itemInstances[index].gameObject);
        itemInstances.RemoveAt(index);
        Vector2 sizeDelta = transform.GetComponent<RectTransform>().sizeDelta;
        this.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, this.length); 
        this.transform.Translate(0f, -addButtonPrefab.GetComponent<RectTransform>().rect.height / 2, 0f);
        for (int i = index; i != itemInstances.Count; ++i){
            itemInstances[i].transform.Translate(0f, -addButtonPrefab.GetComponent<RectTransform>().rect.height, 0f);
        }
    }


    public void notifyChange(){
        int noneindex = itemInstances.FindIndex(b => b.GetComponent<ChooseLaneConfig>().choice == "none");
        if (noneindex > -1 && noneindex < itemInstances.Count - 1)
        {
            deleteChoice(noneindex);
        }

        int notnonecount = (itemInstances.Last().GetComponent<ChooseLaneConfig>().choice != "none") ?
            itemInstances.Count : itemInstances.Count - 1;

        laneconfigresult.Clear();
        //TODO: add UI
        //laneconfigresult.Add("fence");
        for (int i = 0; i != notnonecount; ++i)
        {
            if (i > 0 && itemInstances[i - 1].GetComponent<ChooseLaneConfig>().choice != "lane"){
                laneconfigresult.Add("interval");
            }
            laneconfigresult.Add(itemInstances[i].GetComponent<ChooseLaneConfig>().choice);
        }
        laneconfigresult.Add("surface");
        //laneconfigresult.Add("fence");

        updatePanelDisplay();

        if (itemInstances.Last().GetComponent<ChooseLaneConfig>().choice != "none")
            addNewChoice();
        StartCoroutine(rollOrUnroll(true, this.speed * 5));


    }

    private void updatePanelDisplay()
    {
        Vector2 sizeDelta = transform.GetComponent<RectTransform>().sizeDelta;
        this.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(sizeDelta.x, this.length);
        //this.transform.Translate(0f, addButtonPrefab.GetComponent<RectTransform>().rect.height / 2, 0f);

        for (int i = 0; i != itemInstances.Count; ++i){
            itemInstances[i].GetComponent<RectTransform>().anchoredPosition = new Vector3(0f, lengthToItem(i) + itemInstances[i].GetComponent<RectTransform>().rect.height / 2, 0f);
        }

    }

    IEnumerator rollOrUnroll(bool unroll, float speed){
        if (unroll)
        {
            while (Mathf.Abs(this.transform.position.y - this.length / 2) > 1f)
            {
                this.transform.Translate(0f, speed * (this.length / 2 - transform.position.y) * Time.deltaTime, 0f);
                yield return null;
            }
        }
        else{
            foreach(Button b in itemInstances){
                b.GetComponent<ChooseLaneConfig>().hideChoiceMenu();
            }
            yield return new WaitForSeconds(roll_delay_sec);
            while (Mathf.Abs(min_visible_length - this.length / 2 - transform.position.y) > 1f)
            {
                this.transform.Translate(0f, speed * (min_visible_length - this.length / 2 - transform.position.y) * Time.deltaTime, 0f);
                yield return null;
            }
        }
    }

}
