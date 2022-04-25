using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Game.Actors;
using Mirror;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class PlayerActor : Actor
{
    public float lastAttack;
    public float attackDelay;
    public float attackRange;
    public string attackAnimation = "AttackSword"; 
    public float playTime;

    public int statPoints = 5;
    public int constitution = 7;
    public int dexterity = 5;
    public int luck = 5;
    
    public GameObject ArrowPrefab;
    private Texture2D cursorNormal;
    private Texture2D cursorAttack;
    private Texture2D cursorTalk; 
    
    private Slider ui_targetHealthbar;
    private TMP_Text ui_targetHealth;
    private TMP_Text ui_targetName;
    private Slider ui_playerHealthbar;
    private TMP_Text ui_playerHealth;
    private TMP_Text ui_score;
    private GameObject ui_targetContainer;

    private float pauseTil = 0;

    private Actor lastTarget; 
    
    
    private void Start()
    {
        if (isLocalPlayer)
        {
            gameObject.tag = "Player";
            gameObject.tag = "Player";
            ui_targetContainer = GameObject.Find("UI Target Container"); 
            ui_targetHealthbar = GameObject.Find("Target Healthbar").GetComponent<Slider>();
            ui_playerHealthbar = GameObject.Find("Player Healthbar").GetComponent<Slider>();
            ui_targetHealth = GameObject.Find("Target Health").GetComponent<TMP_Text>();
            ui_targetName = GameObject.Find("Target Name").GetComponent<TMP_Text>();
            ui_playerHealth = GameObject.Find("Player Health").GetComponent<TMP_Text>();
            ui_score = GameObject.Find("Score").GetComponent<TMP_Text>();
            cursorNormal = Resources.Load<Texture2D>("Cursor_Default"); 
            cursorAttack = Resources.Load<Texture2D>("Cursor_Attack"); 
            cursorTalk = Resources.Load<Texture2D>("Cursor_Talk"); 
            Camera.main.GetComponent<SmoothFollow>().Target = this.gameObject.transform;
            GameObject.Find("Minimap Camera").GetComponent<SmoothFollow>().Target = transform;
        }
        maxHealth = constitution * 10;
        health = constitution * 10; 
    }
    private void Update()
    {
        if (isServer)
        {
            if (karma >= 0) team = 0;
            else if (karma < 0) team = 5;
        }
        if (!isLocalPlayer) return;
        playTime += Time.deltaTime;
        
        ui_playerHealth.text = health + " / " + maxHealth;
        ui_playerHealthbar.value = (float) health / (float) maxHealth;
        ui_score.text = "Karma: " + karma;

        if (Time.time < pauseTil)
        {
            movement.isStopped = true; 
            return;
        }
        
        if (movement.isStopped == false)
        {
            if (Utils.ZeroYDistance(transform.position, movement.destination) < 0.05F)
            {
                movement.isStopped = true;
                animator.Play("Idle");
            }
            else
            {
                animator.Play("Running");
            }
        }
        
        RaycastHit hit;
        bool rayHit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);
        if (rayHit)
        {
            Actor actor = hit.collider.gameObject.GetComponent<Actor>();
            NPCTalk talk = hit.collider.gameObject.GetComponent<NPCTalk>();
            if (actor != null && actor != this)
            {
                Cursor.SetCursor(cursorAttack, Vector2.zero, CursorMode.Auto);
            }
            else if (talk != null)
            {
                Cursor.SetCursor(cursorTalk, Vector2.zero, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(cursorNormal, Vector2.zero, CursorMode.Auto);
            }
        }

        if (Input.GetMouseButtonDown(0) && rayHit)
        {
            Debug.Log(hit.collider.gameObject.name);
            Actor actor = hit.collider.gameObject.GetComponent<Actor>();
            NPCTalk talk = hit.collider.gameObject.GetComponent<NPCTalk>();
            if (actor != null && actor != this && actor.health > 0)
            {
                movement.isStopped = true; 
                target = actor;
            }

            if (talk != null)
            {
                Dialogue.Open(talk.text);
                if (talk.audio)
                {
                    AudioSource source = GameObject.Find("Audio Source").GetComponent<AudioSource>();
                    source.clip = talk.audio;
                    source.Play(0);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            target = null; 
            movement.isStopped = false;
            movement.destination = hit.point;
        }
        
        if (target != null)
        {
            if (target.health <= 0)
            {
                target = null;
                return;
            }

            lastTarget = target; 
            
            ui_targetContainer.SetActive(true);
            ui_targetHealth.text = target.health + " / " + target.maxHealth;
            ui_targetHealthbar.value = (float)target.health / (float)target.maxHealth;
            ui_targetName.text = target.gameObject.name;
            
            transform.LookAt(target.transform);
            if (Utils.ZeroYDistance(transform.position, target.transform.position) > attackRange)
            {
                movement.isStopped = false;
                movement.destination = target.transform.position;
                return;
            }
            else
            {
                movement.isStopped = true;
            }

            if (Time.time - lastAttack > attackDelay)
            {
                lastAttack = Time.time;
                animator.Play(attackAnimation);
                pauseTil = Time.time + 0.5F; 
            }
        }
        else
        {
            if (lastTarget != null && lastTarget.health > 0)
            {
                ui_targetContainer.SetActive(true);
                ui_targetHealth.text = lastTarget.health + " / " + lastTarget.maxHealth;
                ui_targetHealthbar.value = (float)lastTarget.health / (float)lastTarget.maxHealth;
                ui_targetName.text = lastTarget.gameObject.name;
            }
            else
            {
                ui_targetContainer.SetActive(false);
            }
        }
    }

    public void Hit()
    {
        if (!isServer) return;
        if(UnityEngine.Random.Range(0, 100) < this.luck)
            target.Damage(this, dexterity * 2);
        else 
            target.Damage(this, dexterity);
    }
    
    public void Shoot()
    {
        if (!isServer) return;
        HomingArrow arrow = Instantiate(ArrowPrefab).GetComponent<HomingArrow>();
        arrow.target = target;
        if(UnityEngine.Random.Range(0, 100) < this.luck)
            arrow.damage = dexterity * 2;
        else 
            arrow.damage = dexterity;
        arrow.shooter = this;
        arrow.transform.position = new Vector3(transform.position.x + (transform.forward.x),
            transform.position.y + 0.65F, transform.position.z + (transform.forward.z));
        NetworkServer.Spawn(arrow.gameObject);
    }

    public void IncreaseStat(string stat)
    {
        if (statPoints <= 0) return;
        switch (stat)
        {
            case "dex":
                dexterity++;
                statPoints--;
                break;
            case "const":
                constitution++;
                statPoints--;
                maxHealth += 10;
                if(health > 0) health += 10;
                break;
            case "luck":
                luck++;
                statPoints--;
                break;
        }
    }

    public void Teleport(Vector3 position)
    {
        movement.Warp(position); 
    }
}
