using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffObjOnTutorialStep : MonoBehaviour, iTutorialType
{

	[SerializeField] GameObject objToHide;
	[SerializeField] bool ShowObjOnTutorialDone;

	public void StepStart(bool ShouldActiveTut)
	{
		objToHide.SetActive(!ShouldActiveTut);
	}

	public void StepDone()
	{
		if (!ShowObjOnTutorialDone) return;
		objToHide.SetActive(true);
	}

	
}
