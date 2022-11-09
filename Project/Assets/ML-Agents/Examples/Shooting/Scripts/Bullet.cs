using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public ShootingAgent agent;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void RegisterAgent(ShootingAgent agent)
    {
        this.agent = agent;
    }

    // Update is called once per frame
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("enemy"))
        {
            col.gameObject.GetComponent<Enemy>().Die();
            GameObject.Destroy(gameObject);
        }
        else if (col.gameObject.CompareTag("max_range"))
        {
            agent.Missed();
            GameObject.Destroy(gameObject);
        }
    }
}
