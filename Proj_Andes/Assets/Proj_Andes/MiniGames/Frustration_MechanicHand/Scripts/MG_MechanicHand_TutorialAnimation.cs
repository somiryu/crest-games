using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_MechanicHand_TutorialAnimation : MonoBehaviour, iTutorialType
{
    [SerializeField] TutorialUser tutorialUser;
    [SerializeField] Animator animator;

    private void Start()
    {
        tutorialUser = gameObject.GetComponentInParent<TutorialUser>();
        animator = gameObject.GetComponent<Animator>();
    }
    public void StepStart(bool stepCompleted)
    {
        gameObject.SetActive(stepCompleted);
        ChangePlayerAnims(tutorialUser.tutorialStep);
    }

    public void StepDone()
    {
        GameUIController.Instance.onTuto = false;
        gameObject.SetActive(false);
    }

    private void ChangePlayerAnims(tutorialSteps step)
    {
        switch (step)
        {
            case tutorialSteps.MG_MechanicHand_1HoldClickAndMove:
                animator.SetTrigger("ClickAndMove");
                break;
            case tutorialSteps.MG_MechanicHand_2JustClickToGrab:
                animator.SetTrigger("JustClick");
                break;        
        }
    }
}


