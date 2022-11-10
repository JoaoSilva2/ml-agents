using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EntityManager : MonoBehaviour
{
    private List<GameObject> entities = new List<GameObject>();
    Bounds m_SpawnAreaBounds;
    public int MAX_ENTITIES;

    public GameObject entity;
    public GameObject trainingArea;

    public ShootingAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        m_SpawnAreaBounds = gameObject.GetComponent<Collider>().bounds;
        this.gameObject.SetActive(false);
        agent.OnEnvironmentReset += RespawnEntities;
    }

    public void EntityDied(GameObject en)
    {
        GameObject.Destroy(en);
        agent.RegisterKill(en.tag);
    }

    void RespawnEntities(object sender, EventArgs e)
    {
        bool enemy = false;

        foreach(var entityClone in entities)
        {
            if (entityClone == null) continue;
            GameObject.Destroy(entityClone);
        }
        entities.Clear();

        int minimum_entities = 1;

        int entityCount = UnityEngine.Random.Range(minimum_entities, MAX_ENTITIES + 1);

        for(int i = 0; i < entityCount; i++)
        {
            var randomPosX = UnityEngine.Random.Range(-m_SpawnAreaBounds.extents.x,
                        m_SpawnAreaBounds.extents.x);
            var randomPosZ = UnityEngine.Random.Range(-m_SpawnAreaBounds.extents.z,
                m_SpawnAreaBounds.extents.z);
            var PosY = 1.0f;

            Vector3 position = new Vector3(randomPosX + gameObject.transform.localPosition.x, PosY, randomPosZ + gameObject.transform.localPosition.z);

            bool needNewPosition = false;

            foreach (var en in entities)
            {
                float distance = Vector3.Distance(en.transform.position, position);
                if (distance < 9f) {
                    needNewPosition = true;
                    break;
                }
            }

            if (needNewPosition)
            {
                i--;
                continue;
            }

            GameObject entityClone = Instantiate(entity);
            entityClone.transform.parent = this.trainingArea.transform;
            entityClone.transform.localPosition = position;

            if(entityClone.CompareTag("enemy"))
            {
                enemy = true;
                entityClone.GetComponent<Enemy>().RegisterToManager(this);
            }
            else
            {
                entityClone.GetComponent<Ally>().RegisterToManager(this);
            }

            entities.Add(entityClone);
        }

        if(enemy)
        {
            agent.enemyCount = entityCount;
        }
        else {
            agent.allyCount = entityCount;
        }
    }
}
