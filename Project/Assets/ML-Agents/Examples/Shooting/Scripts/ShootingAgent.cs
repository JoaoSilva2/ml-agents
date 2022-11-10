using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ShootingAgent : Agent
{
    Rigidbody rBody;
    public Vector3 startingPosition;
    public float bulletSpeed;
    public Rigidbody bullet;
    public Rigidbody currentBullet;
    bool canShoot = true;
    int bulletCount = 3;
    int downedEnemies = 0;
    int downedAllies = 0;

    public int enemyCount = 0;
    public int allyCount = 0;

    public float shootCooldown = 0.3f;
    public bool onCooldown = false;
    public float counter = 0.0f;

    public event EventHandler OnEnvironmentReset;


    void Start () {
        rBody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = startingPosition;
        downedEnemies = 0;  
        downedAllies = 0;
        onCooldown = false;
        if(currentBullet != null)
        {
            GameObject.Destroy(currentBullet.gameObject);
        }
        currentBullet = null;
        canShoot = true;
        counter = 0.0f;
        OnEnvironmentReset?.Invoke(this, EventArgs.Empty);
        bulletCount = enemyCount + 2;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(rBody.velocity.x);
        //sensor.AddObservation(canShoot); //uncomment this for other models
        sensor.AddObservation(bulletCount);
        sensor.AddObservation(enemyCount - downedEnemies); //uncomment this for moving enemies
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        AddReward(-0.00025f);
        int movement = actionBuffers.DiscreteActions[0];
        int shoot = actionBuffers.DiscreteActions[1];

        float directionX = 0.0f;

        // Look up the index in the movement action list:
        if (movement == 0) { directionX = -1; }
        if (movement == 1) { directionX = 1; }
        // Look up the index in the jump action list:
        if (shoot == 0) { Fire(); }

        // Apply the action results to move the Agent
        gameObject.GetComponent<Rigidbody>().AddForce(
            new Vector3(
                directionX * 15f, 0.0f, 0.0f));
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.DiscreteActions;
        continuousActionsOut[0] = (int) Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    void Fire()
    {
        if (!canShoot)
        {
            return;
        }

        Rigidbody bulletClone = (Rigidbody) Instantiate(bullet, transform.position, transform.rotation);
        bulletClone.gameObject.GetComponent<Bullet>().RegisterAgent(this);
        bulletClone.velocity = transform.forward * bulletSpeed;
        currentBullet = bulletClone;

        bulletCount--;
        canShoot = false;

    }

    private void FixedUpdate()
    {
        if(onCooldown)
        {
            counter += Time.deltaTime;
            if(counter >= shootCooldown)
            {
                counter = 0.0f;
                canShoot = true;
                currentBullet = null;
                onCooldown = false;
            }
        }
    }

    public void Missed()
    {
        onCooldown = true;
        if (bulletCount == 0)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
        else
        {
            AddReward(-0.05f / bulletCount);
        }
    }

    public void GameOver()
    {
        SetReward(-1);
        EndEpisode();
    }

    public void RegisterKill(String tag)
    {
        onCooldown = true;
        if(tag.Equals("enemy"))
        {
            AddReward(1.0f / enemyCount - 0.001f);
            downedEnemies++;
            if(downedEnemies == enemyCount)
            {
                SetReward(1f);
                EndEpisode();
            }
        }
        else {
            AddReward(-0.3f / allyCount);
            downedAllies++;
            if(downedAllies == allyCount)
            {
                SetReward(-1f);
                EndEpisode();
            }
        }

        if (bulletCount == 0)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }
}
