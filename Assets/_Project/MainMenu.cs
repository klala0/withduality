using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public NetworkManager Network;
    public TMP_Text ui_leaderboard;
    private void Start()
    {
        Leaderboard.Load();
        string s = "";
        Leaderboard.scores.ForEach(score =>
        {
            s += score + "\n";
        });
        ui_leaderboard.text = s; 
    }

    public void NewGame()
    {
        Network.StartHost();
        Destroy(gameObject);
    }

    public void LoadGame()
    {
        Network.StartHost();
        Invoke(nameof(Delayed), 2F);
    }

    private void Delayed()
    {
        
        GameState.LoadGame();
        Destroy(gameObject);   
    }

    public void JoinGame()
    {
        Network.networkAddress = "localhost";
        Network.StartClient();
        Destroy(gameObject);
    }
    
    
}
