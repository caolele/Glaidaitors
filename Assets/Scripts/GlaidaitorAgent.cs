using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class GlaidaitorAgent : Agent
{

    private Vector3 arenaCenterPosition;

    public GlaidaitorAcademy academy;

    private GameObject agent;

    private Rigidbody agentRigidbody;
    private Vector3 agentCenter; // Used to calculate knockback direction

    RayPerception rayPer;

    private float moveSpeed;
    private float turnSpeed;

    public GameObject platform;


    public override void InitializeAgent()
    {
        base.InitializeAgent();
        this.arenaCenterPosition = Vector3.zero;
        this.agentRigidbody = GetComponent<Rigidbody>();
        rayPer = GetComponent<RayPerception>();
        this.moveSpeed = 0.3f;
        this.turnSpeed = 100f;
        this.agentCenter = findAgentCenter();
    }

    void OnDrawGizmos() {
        if (agentCenter != null) {
            print(agentCenter);
            Gizmos.DrawSphere(agentCenter, 1);
        }
    }

    private Vector3 findAgentCenter() {
        Transform[] ts = transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in ts) {
            if (t.gameObject.name == "limbs") {
                return t.gameObject.GetComponent<BoxCollider>().center;
            }
        }
        print("DIDNT FIND TORSO");
        return new Vector3(-999f, -999f, -999f); // Super hacky, fix

    }

    public override void CollectObservations()
    {
        // Looking around with raycasts
        float rayDistance = 5f;
        float[] rayAngles = { 20f + 180f, 90f + 180f, 160f + 180f, 45f + 180f, 135f + 180f, 70f + 180f, 110f + 180f }  ;
        string[] detectableObjects = { "sword", "shield", "body" };
        AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, -0.1f));

        // Debuging code
        List<string>  localStrings = new List<string>();
        foreach (var ray in (rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 2.5f, -0.1f)))
        {
            localStrings.Add(ray.ToString("R"));
            int lens = localStrings.Count;
            if(lens == 5)
            {
                //Debug.Log(string.Join(",", localStrings));
                localStrings.Clear();
            }

        }
        // The current speed of the agent
        Vector3 localVelocity = transform.InverseTransformDirection(this.agentRigidbody.velocity);
        AddVectorObs(localVelocity.x);
        AddVectorObs(localVelocity.z);

    }


    private void HandleMovement(float[] action) {
		Vector3 dirToGo = Vector3.zero;
		Vector3 rotateDir = Vector3.zero;

		if (brain.brainParameters.vectorActionSpaceType == SpaceType.continuous)
		{
			dirToGo = transform.forward * Mathf.Clamp(action[0], -1f, 1f);
			rotateDir = transform.up * Mathf.Clamp(action[1], -1f, 1f);
		}
		else
		{
			switch ((int)(action[0]))
			{
				case 1:
					dirToGo = -transform.forward;
					break;
				case 2:
					rotateDir = -transform.up;
					break;
				case 3:
					rotateDir = transform.up;
					break;
			}
		}
		agentRigidbody.AddForce(dirToGo * moveSpeed, ForceMode.VelocityChange);
		transform.Rotate(rotateDir, Time.fixedDeltaTime * turnSpeed);

		if (agentRigidbody.velocity.sqrMagnitude > 25f) // slow it down
		{
			agentRigidbody.velocity *= 0.95f;
		}

    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
       HandleMovement(vectorAction);
       checkForDeath();

    }

    private void checkForDeath() {
        float distanceFromArenaCenter = Vector3.Distance(this.transform.position, this.arenaCenterPosition);
        if (distanceFromArenaCenter > platform.transform.localScale.z) {
            Done();
            AddReward(-academy.offTheRingReward);
        }
    }

    void OnCollisionEnter(Collision other) {
        // If we had a cylinder collider we could just use the normal to find contact point?
        print("On collision enter");
        print(other.gameObject.tag);
        if (other.gameObject.CompareTag("sword")) {
            print("Sword hit");
            Vector3 firstPointOfContact = other.contacts[0].point;
            ApplyKnockback(academy.knockBackForce, firstPointOfContact);
        }
    }

    private void ApplyKnockback(float knockBackForce, Vector3 contactPoint) {
        Vector3 knockbackDirection = findKnockbackDirection(contactPoint);
		agentRigidbody.AddForce(knockbackDirection * academy.knockBackForce, ForceMode.VelocityChange);
    }

    private Vector3 findKnockbackDirection(Vector3 contactPoint) {
        return (this.agentCenter - contactPoint).normalized;
    }

    public override void AgentReset()
    {
         Vector3 newPosition = getRandomNewPosition();
         Quaternion newRotation = getRandomNewQuaternionInXZPlane();
    
         transform.position = newPosition;
         transform.rotation = newRotation;
         transform.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
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
