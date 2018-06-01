using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMotion : MonoBehaviour {

    //public int forwardFluence = 100;
    public int backwardFluence = 30;
    public float velocity = 10;
    private int currentIndex = 0;
    private bool negativeDir = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (currentIndex > -backwardFluence)
        {
            currentIndex--;
        }
        else
        {
            negativeDir = !negativeDir;
            currentIndex = 0;

        }        

        if (negativeDir)
        {
            MoveForward();
        }
        else
        {
            MoveBackward();
        }
    }

    void MoveForward()
    {
        transform.Translate(Vector3.up * Time.deltaTime*velocity);
    }

    void MoveBackward()
    {
        transform.Translate(-Vector3.up * Time.deltaTime*velocity);
    }
}
