using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_MechanicHand_TutorialAnimation : MonoBehaviour, iTutorialType
{
    TutorialUser tutorialUser;
    Animator animator;

    private void Start()
    {
        tutorialUser = gameObject.GetComponentInParent<TutorialUser>();
        animator = gameObject.GetComponent<Animator>();
    }
    public void StepStart(bool stepCompleted)
    {
       this.gameObject.SetActive(stepCompleted);
    }

    public void StepDone()
    {
        this.gameObject.SetActive(false);
    }
}


