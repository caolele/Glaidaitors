using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class GlaidaitorAgent : Agent
{

    private Vector3 arenaCenterPosition;

    public GlaidaitorAcademy academy; 

    private GameObject agent;

    private Rigidbody agentRigidbody;

    RayPerception rayPer;


    public override void InitializeAgent()
    {
        base.InitializeAgent();
        this.arenaCenterPosition = Vector3.zero;
        this.agentRigidbody = GetComponent<Rigidbody>();
        rayPer = GetComponent<RayPerception>();
    }

    public override void CollectObservations()
    {
        // Looking around with raycasts
        float rayDistance = 5f;
        float[] rayAngles = { 20f + 180f, 90f + 180f, 160f + 180f, 45f + 180f, 135f + 180f, 70f + 180f, 110f + 180f }  ;
        string[] detectableObjects = { "sword", "shield", "body" };
        AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, -0.1f));


        List<string>  localStrings = new List<string>();
        
        foreach (var ray in (rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 2.5f, -0.1f)))
        {
            localStrings.Add(ray.ToString("R"));
            int lens = localStrings.Count;
            if(lens == 5)
            {
                Debug.Log(string.Join(",", localStrings));
                localStrings.Clear();
            }
            
        }
        // The current speed of the agent
        Vector3 localVelocity = transform.InverseTransformDirection(this.agentRigidbody.velocity);
        AddVectorObs(localVelocity.x);
        AddVectorObs(localVelocity.z);

    }


    private void HandleMovement(float[] action) {


    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
        {
           HandleMovement(vectorAction);
           checkForDeath();
        } else {
            print("STATE SPACE SHOULD BE CONTINUOUS");
        }

    }

    private void checkForDeath() {
        float distanceFromArenaCenter = Vector3.Distance(this.transform.position, this.arenaCenterPosition);

        if (distanceFromArenaCenter > academy.arenaRadius) {
            Done();
            SetReward(-academy.offTheRingReward);
        }
    }

    public override void AgentReset()
    {
        Vector3 newPosition = getRandomNewPosition();
        Quaternion newRotation = getRandomNewQuaternionInXZPlane();
    
        transform.position = newPosition;
        transform.rotation = newRotation;
        transform.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);

        // gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        // ball.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
        // ball.transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 4f, Random.Range(-1.5f, 1.5f)) + gameObject.transform.position;
    }

    private Vector3 getRandomNewPosition() {
        float offsetFromCenter = 2f;// Random.Range(0f, academy.arenaRadius);
        float radians = 0.4f; //Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 newCoord = new Vector3(Mathf.Sin(radians), transform.position.y, Mathf.Cos(radians));
        return offsetFromCenter * newCoord; 
    }

    private Quaternion getRandomNewQuaternionInXZPlane() {
        return Quaternion.Euler(0, Random.Range(0f, 360f), 0);
    }

}
