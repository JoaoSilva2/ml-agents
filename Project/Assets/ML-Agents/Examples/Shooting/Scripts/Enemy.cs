using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    //public ShootingAgent agent;
    private Vector3 StartPosition;
    //public GameObject spawnArea;
    //Bounds m_SpawnAreaBounds;
    private EnemyManager manager;

    // Start is called before the first frame update
    void Start()
    {
        StartPosition = transform.position;
        //m_SpawnAreaBounds = spawnArea.GetComponent<Collider>().bounds;
        //spawnArea.SetActive(false);
        //agent.OnEnvironmentReset += Respawn;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RegisterToManager(EnemyManager mg)
    {
        this.manager = mg;
    }

    //public void Respawn(object sender, EventArgs e)
    //{
    //    gameObject.SetActive(true);
    //    var randomPosX = UnityEngine.Random.Range(-m_SpawnAreaBounds.extents.x,
    //        m_SpawnAreaBounds.extents.x);
    //    var randomPosZ = UnityEngine.Random.Range(-m_SpawnAreaBounds.extents.z,
    //        m_SpawnAreaBounds.extents.z);

    //    transform.position = spawnArea.transform.position + new Vector3(randomPosX, StartPosition.y, randomPosZ);
    //}

    public void Die()
    {
        manager.EnemyDied(this.gameObject);
    }
}
