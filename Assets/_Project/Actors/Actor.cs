using System;
using System.ComponentModel.Design.Serialization;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Actors
{
    public abstract class Actor : NetworkBehaviour
    {
        [SyncVar] public int health;
        [SyncVar] public int maxHealth;
        [SyncVar] public int score;
        [SyncVar] public int karma;
        public int karmaWorth;
        public int scoreWorth;
        [SyncVar] public int team;
        public NavMeshAgent movement;
        public Actor target;
        public Animator animator;
        protected event Action<Actor, int> onDamaged;
        protected event Action<Actor> onDeath;

        protected void Awake()
        {
            movement = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        public void Move(Vector3 destination)
        {
            if (health <= 0) return;
            movement.destination = destination;
            movement.isStopped = false;
        }
        
        public void Damage(Actor source, int amount)
        {
            if (this.health <= 0) return; 
            this.health = Math.Max(0, this.health - amount); 
            onDamaged?.Invoke(source, amount);
            if (this.health <= 0)
            {
                movement.isStopped = true;
                source.AddScore(scoreWorth);
                source.karma += karmaWorth;
                Invoke(nameof(Die), 2.5F);
                onDeath?.Invoke(source);
            }
        }

        public void AddScore(int amount)
        {
            if (gameObject.tag == "Player" && score < 100 && score + amount > 100)
            {
                Leaderboard.Save(((PlayerActor)this).playTime);
            }

            score += amount;
        }
        private void Die()
        {
            Destroy(gameObject);
        }
    }
}