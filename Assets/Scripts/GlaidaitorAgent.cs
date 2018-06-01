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

    public GameObject sword;


    public override void InitializeAgent()
    {
        base.InitializeAgent();
        this.arenaCenterPosition = Vector3.zero;
        this.agentRigidbody = GetComponent<Rigidbody>();
        rayPer = GetComponent<RayPerception>();
        this.moveSpeed = 0.5f;
        this.turnSpeed = 100f;
        this.agentCenter = findAgentCenter();
    }

    void OnDrawGizmos() {
        if (agentCenter != null) {
            Gizmos.DrawSphere(agentCenter, 1);
        }
    }

    private Vector3 findAgentCenter() {
        Transform[] ts = transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in ts) {
            if (t.gameObject.name == "limbs") {
                Vector3 colliderCenter = t.gameObject.GetComponent<BoxCollider>().center;
                return transform.TransformPoint(colliderCenter);
            }
        }
        //print("DIDNT FIND TORSO");
        return new Vector3(-999f, -999f, -999f); // Super hacky, fix

    }

    public override void CollectObservations()
    {
        // Looking around with raycasts
        float rayDistance = 5f;
        float[] rayAngles = { 20f, 90f, 160f, 45f, 135f, 70f, 110f };
        string[] detectableObjects = { "sword", "shield", "body" };
        AddVectorObs(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, -0.1f));

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
       AddReward(0.005f);
       checkForDeath();
    }

    private void checkForDeath() {
        float distanceFromArenaCenter = Vector3.Distance(this.transform.position, this.arenaCenterPosition);

        if (distanceFromArenaCenter > academy.arenaRadius) {
            Done();
            AddReward(-academy.offTheRingReward);
        }
    }

    // This is used to
    void OnCollisionEnter(Collision other) {
        // If we had a cylinder collider we could just use the normal?
        print("On collision enter");
        if (other.gameObject.tag == "arena")
        {
            return;
        }


        Debug.Log(other.gameObject.name);

        List<GameObject> hitGameobjects = FindObjectsWithTag(other.gameObject.transform, "sword");
        foreach (var hitGameobject in hitGameobjects)
        {
            if (this.gameObject.name != hitGameobject.transform.root.gameObject.name)
            {
                print("Sword hit");
                Vector3 averagePoint = Vector3.zero;
                foreach (var point in other.contacts)
                {
                    averagePoint += point.point;
                }
                averagePoint /= other.contacts.Length;
                Vector3 firstPointOfContact = other.contacts[0].point;
                //ApplyKnockback(academy.knockBackForce, firstPointOfContact);
                ApplyKnockback(academy.knockBackForce, averagePoint);
                AddReward(-academy.hitReward);
            }
        }
    }

    public List<GameObject> FindObjectsWithTag(Transform parent, string tag)
    {
        List<GameObject> taggedGameObjects = new List<GameObject>();

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.tag == tag)
            {
                taggedGameObjects.Add(child.gameObject);
            }
            if (child.childCount > 0)
            {
                taggedGameObjects.AddRange(FindObjectsWithTag(child, tag));
            }
        }
        return taggedGameObjects;
    }

    private void ApplyKnockback(float knockBackForce, Vector3 contactPoint) {
        Vector3 knockbackDirection = -findKnockbackDirection(contactPoint);
		agentRigidbody.AddForce(knockbackDirection * academy.knockBackForce, ForceMode.VelocityChange);
    }

    private Vector3 findKnockbackDirection(Vector3 contactPoint) {
        return new Vector3(contactPoint.x - this.agentCenter.x, 0f, contactPoint.z - this.agentCenter.z).normalized;      //(contactPoint - this.agentCenter).normalized;
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
        float offsetFromCenter = Random.Range(0f, academy.arenaRadius/2.0f);
        float radians = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 newCoord = new Vector3(Mathf.Sin(radians), transform.position.y, Mathf.Cos(radians));
        return offsetFromCenter * newCoord;
    }

    private Quaternion getRandomNewQuaternionInXZPlane() {
        return Quaternion.Euler(0, Random.Range(0f, 360f), 0);
    }

}
