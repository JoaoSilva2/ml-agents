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
    int enemieCount = 1;

    public event EventHandler OnEnvironmentReset;


    void Start () {
        rBody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = startingPosition;
        downedEnemies = 0;
        bulletCount = 3;
        OnEnvironmentReset?.Invoke(this, EventArgs.Empty);
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
        if(bulletCount == 0)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f;
    }

    void Fire()
    {
        if (!canShoot)
        {
            return;
        }

        AddReward(-0.1f);
        Rigidbody bulletClone = (Rigidbody) Instantiate(bullet, transform.position, transform.rotation);
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

    public void RegisterKill()
    {
        AddReward(1.0f / enemieCount);
        downedEnemies++;
        if(downedEnemies == enemieCount)
        {
            SetReward(1f);
            EndEpisode();
        }
    }
}
