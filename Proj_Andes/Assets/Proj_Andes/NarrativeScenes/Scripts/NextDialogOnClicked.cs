using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextDialogOnClicked : MonoBehaviour
{
	public void OnClicked() => NarrativeSceneManager.Instance.DialogueDisplayerUI.OnWantsToChangeDialogFromTrigger();
}
