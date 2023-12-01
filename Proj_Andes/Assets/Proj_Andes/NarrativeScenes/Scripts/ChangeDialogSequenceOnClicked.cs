using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeDialogSequenceOnClicked : MonoBehaviour
{
    public DialogueSequenceData sequenceToSet;

	public GameObject[] objToHideOnClick;
	public GameObject[] objToShowOnClick;

	public void OnClicked()
	{
		bool succed = NarrativeSceneManager.Instance.DialogueDisplayerUI.SetPendingSequence(sequenceToSet);
		if (succed)
		{
			for (int i = 0; i < objToHideOnClick.Length; i++) objToHideOnClick[i].gameObject.SetActive(false);
			for (int i = 0; i < objToShowOnClick.Length; i++) objToShowOnClick[i].gameObject.SetActive(true);
		}
	}
}
