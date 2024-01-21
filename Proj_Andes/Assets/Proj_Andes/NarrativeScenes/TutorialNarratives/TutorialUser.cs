using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUser : MonoBehaviour
{
	public tutorialSteps tutorialStep;
    
    iTutorialType tutorialType;

	private void Awake()
	{
        tutorialType = GetComponentInChildren<iTutorialType>(includeInactive: true);
	}

	private void Start()
	{
        TutorialManager.Instance.AddNewUser(this);
    }

    private void OnEnable()
    {
        if(tutorialType == null) tutorialType = GetComponentInChildren<iTutorialType>(includeInactive: true);
      CheckTutorialStepDone();
    }

    private void CheckTutorialStepDone()
    {        var activeTut = !UserDataManager.CurrUser.IsTutorialStepDone(tutorialStep);
        tutorialType.StepStart(activeTut);
    }

    public void OffTutorial()
    {
        if (tutorialType == null) tutorialType = GetComponentInChildren<iTutorialType>(includeInactive: true);
        tutorialType.StepDone();
        UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialStep.ToString());
    }
    
    public void OnTutorial()
    {
        tutorialType.StepStart(true);
    }

    public void SetNewStep(tutorialSteps newTutorialStep)
    {
        tutorialStep = newTutorialStep;
        CheckTutorialStepDone();
    }
}

public interface iTutorialType
{
    public void StepStart(bool stepCompleted);
    public void StepDone();

}
