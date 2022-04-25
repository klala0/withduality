using System.Collections;
using Game.Actors;
using Mirror;
using UnityEngine;

public class Reaper : Actor
{
    public float lastAttack;
    public float attackDelay;
    public float attackRange;
    public string attackAnimation = "AttackBow"; 
    public int damage;
    public NPCActorState state;

    public GameObject ArrowPrefab;
    
    private void Start()
    {
        if (!isServer) return; 
        onDamaged += ((source, amount) =>
        {
            state = NPCActorState.Combat;
            if(target == null) target = source; 
        });

        onDeath += ((source) =>
        {
            PlayerActor[] players = GameObject.FindObjectsOfType<PlayerActor>();
            foreach (var playerActor in players)
            {
                playerActor.Teleport(new Vector3(207.42F, 2.472F, 13.7F));
            }
        });
    }

    
    private void Update()
    {
        if (!isServer) return;
        movement.isStopped = true; 
        
        if (health <= 0)
        {
            animator.Play("dead");
            return;
        }
        
        switch (state)
        {
            case NPCActorState.Idle: 
                animator.Play("idle_normal");
                break;
            
            case NPCActorState.Combat:
                if (target != null)
                {
                    transform.LookAt(target.transform);
                }

                if (target.health <= 0 || Vector3.Distance(this.transform.position, target.transform.position) > this.attackRange)
                    EnemySearch();
                
                if (target != null)
                {
                    if (Time.time - this.lastAttack > this.attackDelay)
                    {
                        animator.Play(attackAnimation);
                        this.lastAttack = Time.time;
                    }
                }
                else
                {
                    state = NPCActorState.Idle;
                }
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
            if (actor.health > 0 && actor.team != this.team && Vector3.Distance(actor.transform.position, transform.position) <= attackRange)
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