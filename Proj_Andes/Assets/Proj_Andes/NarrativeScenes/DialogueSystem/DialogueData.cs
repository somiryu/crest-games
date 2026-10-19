using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

[Serializable]
public class DialogueData
{
	public CharactersTypes characterType;

	[Tooltip("If true, the dialog will continue to the next line automatically when pressing the dialog box, (only works if no responses are present)")]
	public bool autoContinueOnClickDialog = true;

	[Header("Change sequence on continue configs")]
	public DialogueSequenceData changeToSequence;

	public int changeToSequenceStartDialogIdx = -1;

	public int changeToSequenceResponseIdxToGrayOut = -1;

	[TextArea(1, 20)]
	[Space]
	public string text;

	[TextArea(1, 20)]
	public string textAlternative;

	[Header("Responses")]
	public DialoguesResponsesDisplayerUI responsesDisplayerPrefab;

	public DialogueResponse[] responses;

	[Space(20f)]
	public AudioClip audio;

	public AudioClip audioAlternative;

	[Header("Anims")]
	public TimelineAsset EnterAnim;

	public TimelineAsset IdleAnim;

	public TimelineAsset ExitAnim;

	public bool AllResponsesWereGrayOut(List<int> grayedOutResponseIdxes)
	{
		if (responses.Length == 0) return false;
		if (responses.Length == grayedOutResponseIdxes.Count) return true;
		return false;
	}
}
