using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlaidaitorAcademy : Academy
{

    public float knockBackForce;
    public float hitReward;
    public float offTheRingReward;
    public int arenaRadius;

    public override void InitializeAgent()
    {
        this.hitReward = 5.0f;
        
    }

    public override void AcademyReset()
    {

    }

    public override void AcademyStep()
    {

    }
}
