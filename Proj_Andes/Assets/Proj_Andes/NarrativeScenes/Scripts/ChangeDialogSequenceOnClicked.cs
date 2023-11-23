using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeDialogSequenceOnClicked : MonoBehaviour
{
    public DialogueSequenceData sequenceToSet;

    public void OnClicked() => NarrativeSceneManager.Instance.DialogueDisplayerUI.ShowDialogueSequence(sequenceToSet);
}
