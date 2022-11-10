using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Entity : MonoBehaviour
{
    private Vector3 StartPosition;
    protected EntityManager manager;

    // Start is called before the first frame update
    public void Start()
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
