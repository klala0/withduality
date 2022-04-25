using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EscapeMenu : MonoBehaviour
{
    public GameObject container;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            container.SetActive(!container.activeSelf);
        }
    }

    public void Quit()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void SaveQuit()
    {
        GameState.SaveState();
        Quit(); 
    }

    public void Continue()
    {
        container.SetActive(false);
    }
}
