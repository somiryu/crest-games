using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUser : MonoBehaviour
{
	public tutorialSteps tutorialStep;
    
    List<iTutorialType> tutorialsListeners = new List<iTutorialType>();

	private void Awake()
	{
        tutorialsListeners = new List<iTutorialType>(GetComponentsInChildren<iTutorialType>(includeInactive: true));
	}

	private void Start()
	{
        if(TutorialManager.Instance.gameUIController != null) TutorialManager.Instance.gameUIController.TurnOnTurboBtn(true);
        TutorialManager.Instance.AddNewUser(this);
    }

    private void OnEnable()
    {
        if (tutorialsListeners == null || tutorialsListeners.Count == 0)
        {
			tutorialsListeners = new List<iTutorialType>(GetComponentsInChildren<iTutorialType>(includeInactive: true));
		}
        CheckTutorialStepDone();
    }

    private void CheckTutorialStepDone()
    {
        var activeTut = !UserDataManager.CurrUser.IsTutorialStepDone(tutorialStep);
        for (int i = 0; i < tutorialsListeners.Count; i++)
        {
            tutorialsListeners[i].StepStart(activeTut);
        }
    }


    public void OffTutorial()
    {
		if (tutorialsListeners == null || tutorialsListeners.Count == 0)
		{
			tutorialsListeners = new List<iTutorialType>(GetComponentsInChildren<iTutorialType>(includeInactive: true));
		}
		for (int i = 0; i < tutorialsListeners.Count; i++)
		{
			tutorialsListeners[i].StepDone();
		}
        if (TutorialManager.Instance.gameUIController != null) TutorialManager.Instance.gameUIController.TurnOnTurboBtn(false);
		UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialStep.ToString());
    }
    
    public void OnTutorial()
    {
        for (int i = 0; i < tutorialsListeners.Count; i++)
		{
			tutorialsListeners[i].StepStart(true);
		}
    }

    public void SetNewStep(tutorialSteps newTutorialStep)
    {
        tutorialStep = newTutorialStep;
        CheckTutorialStepDone();
    }
}

public interface iTutorialType
{
    public void StepStart(bool ShouldActiveTut);
    public void StepDone();

}
