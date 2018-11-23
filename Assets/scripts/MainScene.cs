using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    public GameObject rend;

    // Use this for initialization
    void Start()
    {
        Curve b = new Bezeir(new Vector2(0f, 0f), new Vector2(10f, 0f), new Vector2(5f, 10f));
        Debug.Log(b.cut(0f, 0.2f));
        Debug.Log(b.cut(0.2f, 0.7f));
        Debug.Log(b.cut(0.7f, 1.0f));


    }
}
