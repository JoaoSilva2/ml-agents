using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnemyManager : MonoBehaviour
{
    private List<GameObject> enemies = new List<GameObject>();
    Bounds m_SpawnAreaBounds;
    public int MAX_ENEMIES;
    public GameObject enemy;
    public GameObject trainingArea;

    public ShootingAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        m_SpawnAreaBounds = gameObject.GetComponent<Collider>().bounds;
        this.gameObject.SetActive(false);
        agent.OnEnvironmentReset += RespawnEnemies;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnemyDied(GameObject en)
    {
        GameObject.Destroy(en);
        agent.RegisterKill();
    }

    void RespawnEnemies(object sender, EventArgs e)
    {
        foreach(var enemyClone in enemies)
        {
            if (enemyClone == null) continue;
            GameObject.Destroy(enemyClone);
        }
        enemies.Clear();

        int minimum_enemies = 1;

        int enemyCount = Mathf.Max(minimum_enemies, MAX_ENEMIES);

        enemyCount = UnityEngine.Random.Range(1, enemyCount + 1);

        for(int i = 0; i < enemyCount; i++)
        {
            var randomPosX = UnityEngine.Random.Range(-m_SpawnAreaBounds.extents.x,
                        m_SpawnAreaBounds.extents.x);
            var randomPosZ = UnityEngine.Random.Range(-m_SpawnAreaBounds.extents.z,
                m_SpawnAreaBounds.extents.z);
            var PosY = 1.0f;

            Vector3 position = new Vector3(randomPosX, PosY, randomPosZ);

            bool needNewPosition = false;

            foreach (var en in enemies)
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

            GameObject enemyClone = Instantiate(enemy);
            enemyClone.transform.parent = this.trainingArea.transform;
            enemyClone.transform.localPosition = position;

            enemyClone.GetComponent<Enemy>().RegisterToManager(this);
            
            enemies.Add(enemyClone);
        }

        agent.enemyCount = enemyCount;

    }

}
