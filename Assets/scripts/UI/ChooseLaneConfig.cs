using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;


public class ChooseLaneConfig : MonoBehaviour{


    public Sprite dashyellowFlatSprite, solidyellowFlatSprite,
    dashwhiteFlatSprite, solidwhiteFlatSprite,
    laneFlatSprite, fenceFlatSprite;

    public float buttonUnrollSpeed;

    public Sprite addSprite;

    public string choice;

    public GameObject gloweffectPrefab;

    GameObject gloweffect;

    public float buttonSpacing;

    List<Button> buttonInstances = new List<Button>();

    bool showingchoicemenu;

	void Start () {
        showingchoicemenu = false;

        for (int i = 1; i != transform.childCount; ++i)
        {
            Button button = transform.GetChild(i).gameObject.GetComponent<Button>();
            disableAndHide(button);
            button.onClick.AddListener(() => setChoice(button.gameObject.name));
            button.onClick.AddListener(() => ShowOrHideChoiceMenu());
            buttonInstances.Add(button);
        }

        GetComponent<Image>().sprite = addSprite;
        this.choice = "none";
    }

    public void ShowOrHideChoiceMenu(){
        if (!showingchoicemenu)
        {
            StopAllCoroutines();
            showingchoicemenu = true;

            for (int i = 1; i <= buttonInstances.Count; ++i)
            {
                float x_offset = buttonInstances.GetRange(0, i).Sum(b => b.GetComponent<RectTransform>().rect.width) + buttonSpacing * i;
                x_offset += buttonInstances[0].GetComponent<RectTransform>().rect.width / 2;
                StartCoroutine(MoveToTaget(i - 1, transform.position.x + x_offset, true));
            }
            StartCoroutine(ExpandToTarget(transform.GetChild(0).gameObject, 
                                          buttonInstances.Sum(b => b.GetComponent<RectTransform>().rect.width) + (buttonInstances.Count + 1) * buttonSpacing));
        }
        else{
            hideChoiceMenu();
        }
    }

    IEnumerator MoveToTaget(int buttonIndex, float target, bool toEnable){
        float originalOffset = Mathf.Abs(target - buttonInstances[buttonIndex].transform.position.x);

        while(Mathf.Abs(buttonInstances[buttonIndex].transform.position.x - target) > 1f){
            buttonInstances[buttonIndex].transform.Translate(buttonUnrollSpeed * (target - buttonInstances[buttonIndex].transform.position.x), 0f, 0f);
            if (Mathf.Abs(target - buttonInstances[buttonIndex].transform.position.x) < originalOffset / 2){
                if (toEnable)
                {
                    buttonInstances[buttonIndex].GetComponent<Image>().enabled = true;
                    buttonInstances[buttonIndex].enabled = true;
                }
                else
                {
                    buttonInstances[buttonIndex].enabled = false;
                    buttonInstances[buttonIndex].GetComponent<Image>().enabled = false;
                }

            }
            yield return new WaitForSeconds(0.1f); 
        }

    }

    IEnumerator ExpandToTarget(GameObject paper, float target){
        float mid_position = target / 2;
        while(Mathf.Abs(paper.GetComponent<RectTransform>().anchoredPosition.x - mid_position) > 1f){
            paper.transform.Translate(buttonUnrollSpeed * (mid_position - paper.GetComponent<RectTransform>().anchoredPosition.x), 0f, 0f);
            paper.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(
                Mathf.Abs(paper.GetComponent<RectTransform>().anchoredPosition.x * 2),
                paper.transform.GetComponent<RectTransform>().sizeDelta.y);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void setChoice(string choice){
        if (choice == "lane"){
            this.GetComponent<Image>().sprite = laneFlatSprite;
        }
        if (choice == "dash_yellow"){
            this.GetComponent<Image>().sprite = dashyellowFlatSprite;
        }
        if (choice == "dash_white"){
            this.GetComponent<Image>().sprite = dashwhiteFlatSprite;
        }
        if (choice == "solid_yellow"){
            this.GetComponent<Image>().sprite = solidyellowFlatSprite;
        }
        if (choice == "solid_white"){
            this.GetComponent<Image>().sprite = solidwhiteFlatSprite;
        }
        if (choice == "fence"){
            this.GetComponent<Image>().sprite = fenceFlatSprite;
        }
        if (choice == "none"){
            this.GetComponent<Image>().sprite = addSprite;
        }
        float hw_ratio = GetComponent<Image>().sprite.rect.height / GetComponent<Image>().sprite.rect.width;
        Vector2 sizedelta = this.GetComponent<RectTransform>().sizeDelta;
        this.GetComponent<RectTransform>().sizeDelta = new Vector2(sizedelta.x, sizedelta.x * hw_ratio);

        this.choice = choice;
        GameObject laneconfigPanel = GameObject.FindWithTag("UI/laneconfig");
        laneconfigPanel.GetComponent<LaneConfigPanelBehavior>().notifyChange();
    }

    void disableAndHide(Button b){
        b.enabled = false;
        b.GetComponent<Image>().enabled = false;
    }

    public void hideChoiceMenu(){
        showingchoicemenu = false;
        StopAllCoroutines();
        for (int i = 0; i < buttonInstances.Count; ++i)
        {
            StartCoroutine(MoveToTaget(i, transform.position.x, false));
        }
        StartCoroutine(ExpandToTarget(transform.GetChild(0).gameObject, 0f));

    }

}
