using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlaidaitorAcademy : Academy
{

    public float knockBackForce;
    public float hitReward;
    public float offTheRingReward;
    [HideInInspector]
    public float arenaRadius;

    public GameObject platform;

    public override void InitializeAcademy()
    {
        this.hitReward = 5.0f;
        this.arenaRadius = platform.transform.localScale.z/2.0f;


    }

    public override void AcademyReset()
    {

    }

    public override void AcademyStep()
    {

    }
}
