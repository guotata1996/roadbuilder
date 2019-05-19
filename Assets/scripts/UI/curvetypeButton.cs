using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Old;

public class curvetypeButton : MonoBehaviour
{
    
    RoadDrawing indicator;

    public void Awake()
    {
        GameObject indicatorInst = GameObject.FindWithTag("Road/curveIndicator");
        indicator = indicatorInst.GetComponent<RoadDrawing>();
    }

    public void Update()
    {
        if (indicator.indicatorType != IndicatorType.none && !EventSystem.current.IsPointerOverGameObject()){
            Vector3 hitpoint = GameObject.FindWithTag("MainCamera").GetComponent<MouseInteraction>().hitpoint3;
            if (Input.GetMouseButtonDown(0)){
                indicator.fixControlPoint(hitpoint);
            }
            else{
                indicator.setControlPoint(hitpoint);
            }
        }

        clearButtonColor();

        switch (indicator.indicatorType){
            case IndicatorType.none:
                break;
            case IndicatorType.line:
                transform.GetChild(0).GetComponent<Image>().color = new Color(83f / 255f, 207f / 255f, 100f / 255f);
                break;
            case IndicatorType.arc:
                transform.GetChild(1).GetComponent<Image>().color = new Color(83f / 255f, 207f / 255f, 100f / 255f);
                break;
            case IndicatorType.bezeir:
                transform.GetChild(2).GetComponent<Image>().color = new Color(83f / 255f, 207f / 255f, 100f / 255f);
                break;
            case IndicatorType.delete:
                transform.GetChild(3).GetComponent<Image>().color = new Color(83f / 255f, 207f / 255f, 100f / 255f);
                break;
        }
    }

    public void setToLineMode()
    {
        clearButtonColor();
        if (indicator.indicatorType == IndicatorType.line)
        {
            indicator.indicatorType = IndicatorType.none;
            indicator.reset();
        }
        else
        {
            indicator.indicatorType = IndicatorType.line;
        }
    }

    public void setToArcMode(){
        clearButtonColor();

        if (indicator.indicatorType == IndicatorType.arc)
        {
            indicator.indicatorType = IndicatorType.none;
            indicator.reset();
        }
        else{
            indicator.indicatorType = IndicatorType.arc;
        }
    }
    public void setToBezierMode(){
        clearButtonColor();

        if (indicator.indicatorType == IndicatorType.bezeir){
            indicator.indicatorType = IndicatorType.none;
            indicator.reset();
        }
        else{
            indicator.indicatorType = IndicatorType.bezeir;
        }
    }

    public void setToDeleteMode(){
        clearButtonColor();
        if (indicator.indicatorType == IndicatorType.delete){
            indicator.indicatorType = IndicatorType.none;
            indicator.reset();
        }
        else{
            indicator.indicatorType = IndicatorType.delete;
        }
    }

    void clearButtonColor(){
        for (int i = 0; i != 4; ++i){
            transform.GetChild(i).GetComponent<Image>().color = Color.white;
        }

    }
}

