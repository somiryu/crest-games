using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetAnimOnTutStep : MonoBehaviour, iTutorialType 
{

	[SerializeField] Animator animator;
	[SerializeField] string animTriggerOnStepStart;
	[SerializeField] string animTriggerOnStepEnd;

	public void StepStart(bool ShouldActiveTut)
	{
		if (ShouldActiveTut)
		{
			if(string.IsNullOrEmpty(animTriggerOnStepStart)) animator.SetTrigger(animTriggerOnStepStart);
		}
		else
		{
			if(string.IsNullOrEmpty(animTriggerOnStepEnd)) animator.SetTrigger(animTriggerOnStepEnd);
		}
	}

	public void StepDone()
	{
		if (string.IsNullOrEmpty(animTriggerOnStepEnd)) animator.SetTrigger(animTriggerOnStepEnd);
	}
}
