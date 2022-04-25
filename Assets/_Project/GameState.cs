using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Game.Actors;
using Mirror;
using UnityEngine;

[Serializable]
public class GameState 
{
[Serializable]
    public class PlayerState
    {
        public SerializedVector position;
        public SerializedQuaternion rotation;
        public float health;
        public float maxHealth;
        public int score;
        public int karma;
    }
    
    [Serializable]
    public class NPCState
    {
        public SerializedVector position;
        public SerializedQuaternion rotation;
        public float health;
        public float maxHealth;
        public string type;
    }

    [Serializable]
    public class SerializedVector
    {
        public float x, y, z;

        public SerializedVector(Vector3 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        public static Vector3 Convert(SerializedVector v)
        {
            return new Vector3(v.x, v.y, v.z); 
        }
    }
    
    [Serializable]
    public class SerializedQuaternion
    {
        public float x, y, z, w;

        public SerializedQuaternion(Quaternion v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
            this.w = v.w;
        }

        public static Quaternion Convert(SerializedQuaternion v)
        {
            return new Quaternion(v.x, v.y, v.z, v.w); 
        }
    }
    
    public List<NPCState> npcStates = new List<NPCState>();
    public PlayerState playerState = null;

    public static void SaveState()
    {
        GameState state = new GameState();
        GameObject[] guards = GameObject.FindGameObjectsWithTag("Guard");
        GameObject player = GameObject.FindWithTag("Player");

        state.npcStates = new List<NPCState>();
        foreach (var gameObject in guards)
        {
            NPCState npcState = new NPCState
            {
                position = new SerializedVector(gameObject.transform.position),
                rotation = new SerializedQuaternion(gameObject.transform.rotation),
                health = gameObject.GetComponent<Actor>().health,
                maxHealth =  gameObject.GetComponent<Actor>().maxHealth,
                type = gameObject.name.Replace("(Clone)", "").Split(" ")[0]
            };
            state.npcStates.Add(npcState);
        }

        state.playerState = new PlayerState
        {
            position = new SerializedVector(player.transform.position),
            rotation = new SerializedQuaternion(player.transform.rotation),
            health = player.GetComponent<Actor>().health,
            maxHealth = player.GetComponent<Actor>().maxHealth,
            karma = player.GetComponent<Actor>().karma,
            score = player.GetComponent<Actor>().score
        };

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gamesave.save");
        bf.Serialize(file, state);
        file.Close();
    }

    [Command(requiresAuthority = false)]
    public static void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/gamesave.save"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gamesave.save", FileMode.Open);
            GameState state = (GameState) bf.Deserialize(file);
            file.Close();
            
            
            GameObject[] guards = GameObject.FindGameObjectsWithTag("Guard");
            GameObject player = GameObject.FindWithTag("Player");
            
            foreach (var gameObject in guards) 
                GameObject.Destroy(gameObject);

            foreach (var stateNpcState in state.npcStates)
            {
                if (stateNpcState.health < 0) continue;
                Debug.Log("Type: " + stateNpcState.type);
                GameObject npc = (GameObject)GameObject.Instantiate(Resources.Load<GameObject>(stateNpcState.type), SerializedVector.Convert(stateNpcState.position), SerializedQuaternion.Convert(stateNpcState.rotation));
                Actor actor = npc.GetComponent<Actor>();
                actor.health = (int)stateNpcState.health;
                actor.maxHealth = (int)stateNpcState.maxHealth;
                NetworkServer.Spawn(npc);
            }

            player.transform.position = SerializedVector.Convert(state.playerState.position);
            player.transform.rotation = SerializedQuaternion.Convert(state.playerState.rotation);
            player.GetComponent<Actor>().health = (int)state.playerState.health;
            player.GetComponent<Actor>().maxHealth = (int)state.playerState.maxHealth;
            player.GetComponent<Actor>().karma = (int)state.playerState.karma;
            player.GetComponent<Actor>().score = (int)state.playerState.score;
        }
    }
}
