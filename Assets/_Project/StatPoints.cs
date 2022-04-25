using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatPoints : MonoBehaviour
{
    public GameObject container;
    public TMP_Text points;
    public TMP_Text constitution;
    public TMP_Text dexterity;
    public TMP_Text luck;
    private PlayerActor player;
    private void Start()
    {
        player = GetComponent<PlayerActor>();
        if (!player.isLocalPlayer)
        {
            Destroy(this);
            return;
        }

        container = GameObject.Find("Stats");
        points = GameObject.Find("Stat Allocation").GetComponent<TMP_Text>();
        constitution = GameObject.Find("Const Label").GetComponent<TMP_Text>();
        dexterity = GameObject.Find("Dex Label").GetComponent<TMP_Text>();
        luck = GameObject.Find("Luck Label").GetComponent<TMP_Text>();

        GameObject.Find("Const").GetComponent<Button>().onClick.AddListener(() => AddConst());
        GameObject.Find("Dex").GetComponent<Button>().onClick.AddListener(() => AddDex());
        GameObject.Find("Luck").GetComponent<Button>().onClick.AddListener(() => AddLuck());
    }

    private void Update()
    {
        if (player.statPoints <= 0)
        {
            container.SetActive(false);
            return;
        }

        points.text = "Stat Points: " + player.statPoints;
        constitution.text = "Add Const (" + player.constitution + ")";
        dexterity.text = "Add Dex (" + player.dexterity + ")";
        luck.text = "Add Luck (" + player.luck + ")";
    }
    
    public void AddConst()
    {
        player.IncreaseStat("const");
    }

    public void AddDex()
    {
        player.IncreaseStat("dex");
    }

    public void AddLuck()
    {
        
        player.IncreaseStat("luck");
    }
}
