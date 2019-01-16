using System.Collections;
using System;
using UnityEngine;
using TMPro;

public class MessageLogger : MonoBehaviour {

    public void LogMessage(string msg){
        transform.GetComponent<TextMeshProUGUI>().SetText(msg);
        transform.localScale = Vector3.one;
        StartCoroutine(WaitAndDo(2f, () => transform.localScale = Vector3.zero));
    }

    IEnumerator WaitAndDo(float time, Action action)
    {
        yield return new WaitForSeconds(time);
        action();
    }
}
