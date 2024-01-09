using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUser : MonoBehaviour
{
    public tutorialSteps tutorialStep;
    [SerializeField] Image tutorialImage;
    [SerializeField] TutorialType tutorialType;

    private void Start()
    {
        TutorialManager.Instance.AddNewUser(this);
        var activeTut = !UserDataManager.CurrUser.IsTutorialStepDone(tutorialStep);
        TryGetComponent(out tutorialType);
        //tutorialImage.gameObject.SetActive(activeTut);
        tutorialType.StepStart(activeTut);
    }

    public void OffTutorial()
    {
        //tutorialImage.gameObject.SetActive(false);
        tutorialType.StepDone();
        UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialStep.ToString());
    }
}

public class TutorialType : MonoBehaviour
{
    public virtual void StepStart(bool stepCompleted)
    {
        if (stepCompleted) return;
    }
    public virtual void StepDone()
    {
    }
}

