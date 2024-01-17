using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_MechanicHand_TutorialAnimation : MonoBehaviour, iTutorialType
{      
    public void StepStart(bool stepCompleted)
    {
       this.gameObject.SetActive(stepCompleted);
    }

    public void StepDone()
    {
        this.gameObject.SetActive(false);

    }
}


