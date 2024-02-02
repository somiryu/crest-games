using UnityEngine;

public class SetObjActiveStateOnTutorialEnd : MonoBehaviour, iTutorialType
{

	[SerializeField] GameObject objToShow;
	[SerializeField] bool stateToSet;
	[SerializeField] bool DisableOnAwake;

	private void Awake()
	{
		if (DisableOnAwake) objToShow.SetActive(false);
	}

	public void StepStart(bool ShouldActiveTut) { }

	public void StepDone()
	{
		objToShow.SetActive(stateToSet);
	}


}

