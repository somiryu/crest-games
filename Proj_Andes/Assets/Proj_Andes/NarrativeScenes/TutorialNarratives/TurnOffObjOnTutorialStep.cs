using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffObjOnTutorialStep : MonoBehaviour, iTutorialType
{

	[SerializeField] GameObject objToHide;
	[SerializeField] bool ShowObjOnTutorialDone;
	[SerializeField] bool HideOnlyOnTutEnd;

	public void StepStart(bool ShouldActiveTut)
	{
		if(ShouldActiveTut && !HideOnlyOnTutEnd) objToHide.SetActive(false);
		if(!ShouldActiveTut && HideOnlyOnTutEnd) objToHide.SetActive(false);
		if (!ShouldActiveTut && ShowObjOnTutorialDone) objToHide.SetActive(true);

	}

	public void StepDone()
	{
		if (ShowObjOnTutorialDone) objToHide.SetActive(true);
		if(HideOnlyOnTutEnd) objToHide.SetActive(false);
	}
}

