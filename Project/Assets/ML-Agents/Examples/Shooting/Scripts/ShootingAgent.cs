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
    //TODO Make enemyManager that can handle different enemies
    public int enemyCount = 0;

    public event EventHandler OnEnvironmentReset;


    void Start () {
        rBody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = startingPosition;
        downedEnemies = 0;        
        OnEnvironmentReset?.Invoke(this, EventArgs.Empty);
        bulletCount = enemyCount + 2;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(canShoot);
        sensor.AddObservation(bulletCount);
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
        if(currentBullet == null)
        {
            canShoot = true;
        }
    }

    public void Missed()
    {
        if (bulletCount == 0)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
        else
        {
            AddReward(-0.1f / bulletCount);
        }
    }

    public void RegisterKill()
    {
        AddReward(1.0f / enemyCount);
        downedEnemies++;
        if(downedEnemies == enemyCount)
        {
            SetReward(1f);
            EndEpisode();
        }
        else if (bulletCount == 0)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }
}
