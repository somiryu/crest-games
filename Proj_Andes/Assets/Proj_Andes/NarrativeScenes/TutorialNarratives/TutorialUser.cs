using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUser : MonoBehaviour, iTutorialUser
{
    public tutorialSteps tutorialStep;
    [SerializeField] Image tutorialImage;    


    private void Start()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.AddNewUser(this);
            if (TutorialManager.Instance.stepsTutorialNarrativeScenes[tutorialStep.ToString()] == false)
                tutorialImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("TutorialManager.Instance is null.");
        }
    }

    public void TurnOffTutorialStep()
    {
        tutorialImage.gameObject.SetActive(false);
        TutorialManager.Instance.stepsTutorialNarrativeScenes[tutorialStep.ToString()] = true;
    }

    public void OffTutorial(tutorialSteps _tutorialStep)
    {
        if (tutorialStep == _tutorialStep)
        {
            tutorialImage.gameObject.SetActive(false);
            TutorialManager.Instance.stepsTutorialNarrativeScenes[tutorialStep.ToString()] = true;
        }
    }
}


