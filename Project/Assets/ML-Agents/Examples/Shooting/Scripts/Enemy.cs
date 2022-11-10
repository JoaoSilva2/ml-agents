using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy : Entity
{
    public float enemySpeed;
    new void Start()
    {
        base.Start();
        gameObject.GetComponent<Rigidbody>().velocity = transform.forward * enemySpeed;
    }

    void Update()
    {
        gameObject.GetComponent<Rigidbody>().velocity = transform.forward * enemySpeed;
    }

    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.CompareTag("player_area"))
        {
            manager.ReachedPlayer();
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("wall"))
        {
            Physics.IgnoreCollision(col.collider, gameObject.GetComponent<Collider>());
        }
        
    }
}
