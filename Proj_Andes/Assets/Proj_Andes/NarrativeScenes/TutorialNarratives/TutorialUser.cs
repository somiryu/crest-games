using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUser : MonoBehaviour
{
    public tutorialSteps tutorialStep;
    [SerializeField] Image tutorialImage;


    private void Start()
    {
        TutorialManager.Instance.AddNewUser(this);
        var activeTut = !UserDataManager.CurrUser.IsTutorialStepDone(tutorialStep);
        tutorialImage.gameObject.SetActive(activeTut);
    }

    public void OffTutorial()
    {
        tutorialImage.gameObject.SetActive(false);
        UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialStep.ToString());
    }
}


