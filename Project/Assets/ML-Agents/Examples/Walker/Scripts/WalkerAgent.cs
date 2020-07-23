using System;
using MLAgentsExamples;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using BodyPart = Unity.MLAgentsExamples.BodyPart;
using Random = UnityEngine.Random;

public class WalkerAgent : Agent
{
    [Header("Walk Speed")]
    [Range(0, 15)]
    public float walkingSpeed = 15; //The walking speed to try and achieve
    float m_maxWalkingSpeed = 15; //The max walking speed
    public bool randomizeWalkSpeedEachEpisode;
    
    Vector3 m_WalkDir; //Direction to the target
    public AnimationCurve velocityRewardCurve;
    [Header("Target To Walk Towards")]
    public TargetController target; //Target the agent will walk towards.

    [Header("Body Parts")] public Transform hips;
    public Transform chest;
    public Transform spine;
    public Transform head;
    public Transform thighL;
    public Transform shinL;
    public Transform footL;
    public Transform thighR;
    public Transform shinR;
    public Transform footR;
    public Transform armL;
    public Transform forearmL;
    public Transform handL;
    public Transform armR;
    public Transform forearmR;
    public Transform handR;

    [Header("Orientation")] [Space(10)]
    //This will be used as a stabilized model space reference point for observations
    //Because ragdolls can move erratically during training, using a stabilized reference transform improves learning
    public OrientationCubeController orientationCube;

    JointDriveController m_JdController;

    EnvironmentParameters m_ResetParams;
//    private WalkGroup walkGroup;
    public override void Initialize()
    {
        orientationCube.UpdateOrientation(hips, target.transform);
//        walkGroup = FindObjectOfType<WalkGroup>();
        //Setup each body part
        m_JdController = GetComponent<JointDriveController>();
        m_JdController.SetupBodyPart(hips);
        m_JdController.SetupBodyPart(chest);
        m_JdController.SetupBodyPart(spine);
        m_JdController.SetupBodyPart(head);
        m_JdController.SetupBodyPart(thighL);
        m_JdController.SetupBodyPart(shinL);
        m_JdController.SetupBodyPart(footL);
        m_JdController.SetupBodyPart(thighR);
        m_JdController.SetupBodyPart(shinR);
        m_JdController.SetupBodyPart(footR);
        m_JdController.SetupBodyPart(armL);
        m_JdController.SetupBodyPart(forearmL);
        m_JdController.SetupBodyPart(handL);
        m_JdController.SetupBodyPart(armR);
        m_JdController.SetupBodyPart(forearmR);
        m_JdController.SetupBodyPart(handR);

        m_ResetParams = Academy.Instance.EnvironmentParameters;

        SetResetParameters();
    }

    /// <summary>
    /// Loop over body parts and reset them to initial conditions.
    /// </summary>
    public override void OnEpisodeBegin()
    {
        //Reset all of the body parts
        foreach (var bodyPart in m_JdController.bodyPartsDict.Values)
        {
            bodyPart.Reset(bodyPart);
        }

        //Random start rotation to help generalize
        transform.rotation = Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0);

        orientationCube.UpdateOrientation(hips, target.transform);

        rewardManager.ResetEpisodeRewards();
        
        walkingSpeed = randomizeWalkSpeedEachEpisode? Random.Range(0.0f, m_maxWalkingSpeed): walkingSpeed; //Random Walk Speed

        SetResetParameters();
    }

    /// <summary>
    /// Add relevant information on each body part to observations.
    /// </summary>
    public void CollectObservationBodyPart(BodyPart bp, VectorSensor sensor)
    {
        //GROUND CHECK
        sensor.AddObservation(bp.groundContact.touchingGround); // Is this bp touching the ground

        //Get velocities in the context of our orientation cube's space
        //Note: You can get these velocities in world space as well but it may not train as well.
        sensor.AddObservation(orientationCube.transform.InverseTransformDirection(bp.rb.velocity));
        sensor.AddObservation(orientationCube.transform.InverseTransformDirection(bp.rb.angularVelocity));

        //Get position relative to hips in the context of our orientation cube's space
        sensor.AddObservation(orientationCube.transform.InverseTransformDirection(bp.rb.position - hips.position));

        if (bp.rb.transform != hips && bp.rb.transform != handL && bp.rb.transform != handR)
        {
            sensor.AddObservation(bp.rb.transform.localRotation);
            sensor.AddObservation(bp.currentStrength / m_JdController.maxJointForceLimit);
        }
    }

    /// <summary>
    /// Loop over body parts to add them to observation.
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {

        var cubeForward = orientationCube.transform.forward;
//        avgVelValue = GetVelocity();

//        sensor.AddObservation(VelocityInverseLerp(cubeForward * walkingSpeed, avgVelValue));


        //current ragdoll velocity. normalized 
//        sensor.AddObservation(VelocityInverseLerp( cubeForward * walkGroup.walkingSpeed)); //
        sensor.AddObservation(VelocityInverseLerp( cubeForward * walkingSpeed));
        
        
        
        //current speed goal. normalized.
        sensor.AddObservation(walkingSpeed/m_maxWalkingSpeed);
//        sensor.AddObservation(walkGroup.walkingSpeed/walkGroup.m_maxWalkingSpeed);
        sensor.AddObservation(Quaternion.FromToRotation(hips.forward, cubeForward));
        sensor.AddObservation(Quaternion.FromToRotation(head.forward, cubeForward));

        sensor.AddObservation(orientationCube.transform.InverseTransformPoint(target.transform.position));

        foreach (var bodyPart in m_JdController.bodyPartsList)
        {
            CollectObservationBodyPart(bodyPart, sensor);
        }
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        var bpDict = m_JdController.bodyPartsDict;
        var i = -1;

        bpDict[chest].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);
        bpDict[spine].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);

        bpDict[thighL].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
        bpDict[thighR].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
        bpDict[shinL].SetJointTargetRotation(vectorAction[++i], 0, 0);
        bpDict[shinR].SetJointTargetRotation(vectorAction[++i], 0, 0);
        bpDict[footR].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);
        bpDict[footL].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], vectorAction[++i]);

        bpDict[armL].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
        bpDict[armR].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);
        bpDict[forearmL].SetJointTargetRotation(vectorAction[++i], 0, 0);
        bpDict[forearmR].SetJointTargetRotation(vectorAction[++i], 0, 0);
        bpDict[head].SetJointTargetRotation(vectorAction[++i], vectorAction[++i], 0);

        //update joint strength settings
        bpDict[chest].SetJointStrength(vectorAction[++i]);
        bpDict[spine].SetJointStrength(vectorAction[++i]);
        bpDict[head].SetJointStrength(vectorAction[++i]);
        bpDict[thighL].SetJointStrength(vectorAction[++i]);
        bpDict[shinL].SetJointStrength(vectorAction[++i]);
        bpDict[footL].SetJointStrength(vectorAction[++i]);
        bpDict[thighR].SetJointStrength(vectorAction[++i]);
        bpDict[shinR].SetJointStrength(vectorAction[++i]);
        bpDict[footR].SetJointStrength(vectorAction[++i]);
        bpDict[armL].SetJointStrength(vectorAction[++i]);
        bpDict[forearmL].SetJointStrength(vectorAction[++i]);
        bpDict[armR].SetJointStrength(vectorAction[++i]);
        bpDict[forearmR].SetJointStrength(vectorAction[++i]);
    }

    void FixedUpdate()
    {
        UpdateRewards();
    }

    Vector3 GetVelocity()
    { 
        Vector3 velSum = Vector3.zero;
        Vector3 avg = Vector3.zero;
        
//        velSum += m_JdController.bodyPartsDict[hips].rb.velocity;
//        velSum += m_JdController.bodyPartsDict[spine].rb.velocity;
//        velSum += m_JdController.bodyPartsDict[chest].rb.velocity;
//        velSum += m_JdController.bodyPartsDict[head].rb.velocity;
//        avg = velSum/4;
        
        //ALL RBS
        int counter = 0;
        foreach (var item in m_JdController.bodyPartsList)
        {
            counter++;
            velSum += item.rb.velocity;
        }
        avg = velSum/counter;
        return avg;
        
//        velInverseLerpVal = VelocityInverseLerp(cubeForward * walkingSpeed, avgVelValue);
    }

    public float velInverseLerpVal;
    public float hipsVelMag;
    public float lookAtTargetReward; //reward for looking at the target
    public float matchSpeedReward; //reward for matching the desired walking speed.
    public float headHeightOverFeetReward; //reward for standing up straight-ish
    public float hurryUpReward = -1; //don't waste time
    public RewardManager rewardManager;
    public float bpVelPenaltyThisStep = 0;
    void UpdateRewards()
    {
        var cubeForward = orientationCube.transform.forward;
        orientationCube.UpdateOrientation(hips, target.transform);
        // Set reward for this step according to mixture of the following elements.
        // a. Match target speed
        //This reward will approach 1 if it matches and approach zero as it deviates
//        matchSpeedReward =
//            Mathf.Exp(-0.1f * (cubeForward * walkingSpeed -
//                               m_JdController.bodyPartsDict[hips].rb.velocity).sqrMagnitude);

        hipsVelMag = m_JdController.bodyPartsDict[hips].rb.velocity.magnitude;
//        velInverseLerpVal =
//            Mathf.InverseLerp(0, walkingSpeed, m_JdController.bodyPartsDict[hips].rb.velocity.magnitude);
        
//        var moveTowardsTargetReward = Vector3.Dot(cubeForward,
//            Vector3.ClampMagnitude(m_JdController.bodyPartsDict[hips].rb.velocity, maximumWalkingSpeed));
        // b. Rotation alignment with goal direction.
//        lookAtTargetReward = Vector3.Dot(cubeForward, head.forward);
        lookAtTargetReward = (Vector3.Dot(cubeForward, head.forward) + 1) * .5F;
//        lookAtTargetReward =
//            Mathf.Exp(-0.1f * (cubeForward * walkingSpeed -
//                               m_JdController.bodyPartsDict[hips].rb.velocity).sqrMagnitude);
        // c. Encourage head height.
        headHeightOverFeetReward =
            Mathf.Clamp01(((head.position.y - footL.position.y) + (head.position.y - footR.position.y))/ 10); //Should normalize to ~1
//        AddReward(
//            +0.02f * moveTowardsTargetReward
//            + 0.01f * lookAtTargetReward
//            + 0.01f * headHeightOverFeetReward
//        );

//        rewardManager.UpdateReward("matchSpeed", matchSpeedReward);
//        rewardManager.UpdateReward("lookAtTarget", lookAtTargetReward);
//        rewardManager.UpdateReward("headHeightOverFeet", headHeightOverFeetReward);
//        rewardManager.UpdateReward("hurryUp", hurryUpReward/MaxStep);

//        //VELOCITY REWARDS
//        bpVelPenaltyThisStep = 0;
//        foreach (var item in m_JdController.bodyPartsList)
//        {
//            var velDelta = Mathf.Clamp(item.rb.velocity.magnitude - walkingSpeed, 0, 1);
//            bpVelPenaltyThisStep += velDelta;
//        }
//        rewardManager.UpdateReward("bpVel", bpVelPenaltyThisStep);


//        Vector3 velSum = Vector3.zero;
//        avgVelValue = Vector3.zero;
//        velSum += m_JdController.bodyPartsDict[hips].rb.velocity;
//        velSum += m_JdController.bodyPartsDict[spine].rb.velocity;
//        velSum += m_JdController.bodyPartsDict[chest].rb.velocity;
//        velSum += m_JdController.bodyPartsDict[head].rb.velocity;
//        avgVelValue = velSum/4;
//        velInverseLerpVal = VelocityInverseLerp(cubeForward * walkingSpeed, avgVelValue);
        velInverseLerpVal = VelocityInverseLerp(cubeForward * walkingSpeed);
        rewardManager.rewardsDict["matchSpeed"].rewardThisStep = velInverseLerpVal;
        rewardManager.rewardsDict["lookAtTarget"].rewardThisStep = lookAtTargetReward;
        rewardManager.rewardsDict["headHeightOverFeet"].rewardThisStep = headHeightOverFeetReward;
//        velInverseLerpVal = VelocityInverseLerp(cubeForward * walkGroup.walkingSpeed);
        rewardManager.UpdateReward("productOfAllRewards", velInverseLerpVal * lookAtTargetReward);
//        rewardManager.UpdateReward("productOfAllRewards", velInverseLerpVal * lookAtTargetReward * headHeightOverFeetReward);
//            velInverseLerpVal = VelocityInverseLerp(Vector3.zero, cubeForward * walkingSpeed, avgVelValue);

        //This reward will approach 1 if it matches and approach zero as it deviates
//        velInverseLerpVal =
//            Mathf.InverseLerp(0, walkingSpeed, avgVelValue.magnitude);
//        rewardManager.UpdateReward("productOfAllRewards", velInverseLerpVal * lookAtTargetReward * headHeightOverFeetReward);
//        matchSpeedReward =
//            Mathf.Exp(-0.1f * (cubeForward * walkingSpeed -
//                               avgVelValue).sqrMagnitude);

//        matchSpeedReward =
//            Mathf.Exp(-0.01f * (cubeForward * walkingSpeed -
//                               avgVelValue).sqrMagnitude);
//        rewardManager.UpdateReward("productOfAllRewards", matchSpeedReward * lookAtTargetReward * headHeightOverFeetReward);
        
        
//        Vector3 velSum = Vector3.zero;
//
//        int counter = 0;
//        avgVelValue = Vector3.zero;
//        foreach (var item in m_JdController.bodyPartsList)
//        {
//            counter++;
//            velSum += item.rb.velocity;
//        }
//            avgVelValue = velSum/counter;
//        //This reward will approach 1 if it matches and approach zero as it deviates
//        matchSpeedReward =
//            Mathf.Exp(-0.1f * (cubeForward * walkingSpeed -
//                               avgVelValue).sqrMagnitude);
//        rewardManager.UpdateReward("productOfAllRewards", matchSpeedReward * lookAtTargetReward * headHeightOverFeetReward);
        
    }
    public Vector3 bodyVelocity;
    //value of 0 means we are matching velocity perfectly
    //value of 1 means we are not matching velocity
    public float velDeltaDistance; //distance between the goal and actual vel
    
    //normalized value of the difference in avg speed vs goal walking speed.
    public float VelocityInverseLerp(Vector3 velocityGoal)
    {
        bodyVelocity = GetVelocity();

//        velDeltaDistance = Vector3.Distance(avgVelValue, velocityGoal);
//        velDeltaDistance = Vector3.Distance(avgVelValue, velocityGoal);
//        velDeltaDistance = Mathf.Clamp(Vector3.Distance(bodyVelocity, velocityGoal), 0, walkGroup.walkingSpeed);
        velDeltaDistance = Mathf.Clamp(Vector3.Distance(bodyVelocity, velocityGoal), 0, walkingSpeed);
//        float percent = Mathf.InverseLerp(walkingSpeed, 0, velDeltaDistance);
//        float percent = Mathf.InverseLerp(velocityGoal.magnitude, 0, velDeltaDistance);
//        float percent = Mathf.Pow(1 - Mathf.Pow(velDeltaDistance/walkGroup.walkingSpeed, 2), 2);
        float percent = Mathf.Pow(1 - Mathf.Pow(velDeltaDistance/walkingSpeed, 2), 2);
        return percent;
    }
//    public float VelocityInverseLerp(Vector3 velocityGoal)
//    {
//        avgVelValue = GetVelocity();
//
//        velDeltaDistance = Vector3.Distance(avgVelValue, velocityGoal);
////        float percent = Mathf.InverseLerp(walkingSpeed, 0, velDeltaDistance);
//        float percent = Mathf.InverseLerp(velocityGoal.magnitude, 0, velDeltaDistance);
//        return percent;
//    }
    
//    public float VelocityInverseLerp(Vector3 velocityGoal, Vector3 currentVel)
//    {
//        avgVelValue = GetVelocity();
//
//        velDeltaDistance = Vector3.Distance(currentVel, velocityGoal);
//        float percent = Mathf.InverseLerp(walkingSpeed, 0, velDeltaDistance);
//        return percent;
//    }
    
//    public float VelocityInverseLerp(Vector3 a, Vector3 b, Vector3 value)
//    {
//        Vector3 AB = b - a;
//        Vector3 AV = value - a;
//        return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
//    }
//    void FixedUpdate()
//    {
//        var cubeForward = orientationCube.transform.forward;
//        orientationCube.UpdateOrientation(hips, target.transform);
//        // Set reward for this step according to mixture of the following elements.
//        // a. Velocity alignment with goal direction.
//        var moveTowardsTargetReward = Vector3.Dot(cubeForward,
//            Vector3.ClampMagnitude(m_JdController.bodyPartsDict[hips].rb.velocity, maximumWalkingSpeed));
//        // b. Rotation alignment with goal direction.
//        var lookAtTargetReward = Vector3.Dot(cubeForward, head.forward);
//        // c. Encourage head height. //Should normalize to ~1
//        var headHeightOverFeetReward = 
//            ((head.position.y - footL.position.y) + (head.position.y - footR.position.y) / 10);
//        AddReward(
//            + 0.02f * moveTowardsTargetReward
//            + 0.02f * lookAtTargetReward
//            + 0.005f * headHeightOverFeetReward
//        );
//    }

    /// <summary>
    /// Agent touched the target
    /// </summary>
    public void TouchedTarget()
    {
        AddReward(1f);
    }

    public void SetTorsoMass()
    {
        m_JdController.bodyPartsDict[chest].rb.mass = m_ResetParams.GetWithDefault("chest_mass", 8);
        m_JdController.bodyPartsDict[spine].rb.mass = m_ResetParams.GetWithDefault("spine_mass", 8);
        m_JdController.bodyPartsDict[hips].rb.mass = m_ResetParams.GetWithDefault("hip_mass", 8);
    }

    public void SetResetParameters()
    {
        SetTorsoMass();
    }
}