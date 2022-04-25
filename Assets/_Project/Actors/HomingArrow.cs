using System;
using Game.Actors;
using Mirror;
using UnityEngine;

public class HomingArrow : NetworkBehaviour
{
    public Actor target;
    public Actor shooter;
    public int damage;
    
    private void Start()
    {
        Invoke("Destruct", 5F);
    }

    private void Destruct()
    {
        Destroy(gameObject);
    }
    
    public void Update()
    {
        if (!isServer) return;
        if (!target || target.health <= 0)
        {
            Destruct();
            return;
        }
        transform.LookAt(target.transform);
        transform.eulerAngles = new Vector3(-90, transform.eulerAngles.y, transform.eulerAngles.z + 180);
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.transform.position.x, target.transform.position.y + 0.65F, target.transform.position.z), 15 * Time.deltaTime);
        if (Vector3.Distance(transform.position, new Vector3(target.transform.position.x, target.transform.position.y + 0.65F, target.transform.position.z)) < 0.1F)
        {
            target.Damage(shooter, damage);
            Destroy(gameObject);
        }
    }
}
