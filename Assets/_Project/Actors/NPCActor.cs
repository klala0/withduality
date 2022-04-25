using System;
using System.Collections;
using System.Collections.Generic;
using Game.Actors;
using Mirror;
using UnityEngine;

public class NPCActor : Actor
{
    public float lastAttack;
    public float attackDelay;
    public float attackRange;
    public float aggroRadius;
    public string attackAnimation = "AttackSword"; 
    public int damage;
    public Vector3 lastPatrolPosition;
    public NPCActorState state;
    public float idleDelay = 3;
    public float lastIdle = 0;

    public GameObject ArrowPrefab;
    public PathScript pathScript;
    
    private void Start()
    {
        if (!isServer) return; 
        onDamaged += ((source, amount) =>
        {
            state = NPCActorState.Combat;
            target = source; 
        });

        onDeath += ((source) =>
        {
            PlayerActor[] players = GameObject.FindObjectsOfType<PlayerActor>();
            int score = 0;
            foreach (var playerActor in players)
            {
                score += playerActor.score;
            }

            if (score >= 100)
            {
                foreach (var playerActor in players)
                {
                    playerActor.Teleport(new Vector3(394.332F, 0.574F, 29.9F)); 
                }
            }
        });
    }

    
    private void Update()
    {
        if (!isServer) return;

        if (health <= 0)
        {
            animator.Play("Die");
            return;
        }
        
        switch (state)
        {
            case NPCActorState.Idle: 
                animator.Play("Idle");
                if (Time.time - this.lastIdle > this.idleDelay)
                    state = NPCActorState.Patrol;
                break;
            
            case NPCActorState.Patrol:
                animator.Play("Walking");
                movement.isStopped = false;
                movement.destination = pathScript.Current().transform.position;
                
                if (Utils.ZeroYDistance(transform.position, movement.destination) < 0.5F)
                    movement.destination = pathScript.Next().transform.position;
                
                if (EnemySearch())
                    lastPatrolPosition = transform.position; 
                break;
            
            case NPCActorState.Combat:
                if (target == null || target.health <= 0)
                {
                    state = NPCActorState.ReturnToPosition;
                    break;
                }
                
                transform.LookAt(target.transform);

                if (Vector3.Distance(this.transform.position, target.transform.position) > this.attackRange)
                {
                    movement.destination = target.transform.position;
                    movement.isStopped = false;
                    animator.Play("Running");
                    break;
                }
                else
                {
                    movement.isStopped = true;
                }
                
                if (Time.time - this.lastAttack > this.attackDelay)
                {
                    movement.isStopped = true; 
                    animator.Play(attackAnimation);
                    this.lastAttack = Time.time;
                }
                break;
            
            case NPCActorState.ReturnToPosition:
                animator.Play("Running");
                movement.isStopped = false;
                movement.destination = lastPatrolPosition;
                state = NPCActorState.Idle;
                break;
        }    
    }

    public void Shoot()
    {
        if (!isServer) return;
        HomingArrow arrow = Instantiate(ArrowPrefab).GetComponent<HomingArrow>();
        arrow.target = target;
        arrow.damage = damage;
        arrow.shooter = this;
        arrow.transform.position = new Vector3(transform.position.x + (transform.forward.x),
            transform.position.y + 0.65F, transform.position.z + (transform.forward.z));
        NetworkServer.Spawn(arrow.gameObject);
    }

    public void Hit()
    {
        if (!isServer) return;
        target.Damage(this, damage); 
    }

    protected IEnumerator DelayDamage(Actor target, int amount)
    {
        yield return new WaitForSeconds(0.65F); 
        if (target != null && target.health > 0)
        {
            target.Damage(this, amount);
        }
    }

    private bool EnemySearch()
    {
        Actor[] actors = GameObject.FindObjectsOfType<Actor>();
        foreach (var actor in actors)
        {
            if (actor.health > 0 && actor.team != this.team && Vector3.Distance(actor.transform.position, transform.position) < aggroRadius)
            {
                target = actor;
                state = NPCActorState.Combat;
                return true;
            }
        }

        return false;
    }
    
    public enum NPCActorState
    {
        Idle,
        Patrol,
        Combat,
        ReturnToPosition
    }
}