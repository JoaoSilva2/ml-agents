using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Entity : MonoBehaviour
{
    private Vector3 StartPosition;
    private EntityManager manager;

    // Start is called before the first frame update
    void Start()
    {
        StartPosition = transform.position;
    }

    public void RegisterToManager(EntityManager mg)
    {
        this.manager = mg;
    }

    public void Die()
    {
        manager.EntityDied(this.gameObject);
    }
}
