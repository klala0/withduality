using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    public static Dialogue Singleton;
    public GameObject panel;
    public TMP_Text text;

    private void Start()
    {
        if (Singleton != null)
        {
            Destroy(this);
            return;
        }

        Singleton = this;
    }
    
    public void Close()
    {
        panel.SetActive(false);    
    }

    public static void Open(string _text)
    {
        Singleton.text.text = _text; 
        Singleton.panel.SetActive(true);
    }
}