using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Spawner : NetworkBehaviour
{
    public bool DelayFirstSpawn;
    public float DelayBetweenSpawns;
    public int SpawnCount;
    public GameObject Prefab;
    private int Spawned; 
    private bool Started;
    private float LastSpawn;

    public void Start()
    {
        if (!isServer) return;
        Started = true;
        if (DelayFirstSpawn) LastSpawn = Time.time; 
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position, new Vector3(1,1,1));
    }

    private void Update()
    {
        if (!Started) return;
        if (Spawned >= SpawnCount)
        {
            Destroy(gameObject);
            return;
        }
        
        if (Time.time - LastSpawn >= DelayBetweenSpawns)
        {
            LastSpawn = Time.time;
            GameObject go = (GameObject)Instantiate(Prefab);
            go.transform.position = this.transform.position; 
            NetworkServer.Spawn(go);
            Spawned++;
        }
    }
}
