using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName = "Dialogue Sequence Data", menuName = "DialogueSystem/Sequence")]
public class DialogueSequenceData : ScriptableObject {
    public DialogueData[] dialogues;

    public override string ToString()
    {
        if (dialogues != null && dialogues.Length > 0) {
            DialogueData firstLine = dialogues[0];
            return $"{firstLine.characterType}: {firstLine.text}";
        }
        return name;
    }
}

[Serializable]
public class DialogueData
{
    public CharactersTypes characterType;
	public bool autoContinueOnClickDialog = true;
    [Header("Change sequence on continue configs")]
    public DialogueSequenceData changeToSequence;
    public int changeToSequenceStartDialogIdx = -1;
    public int changeToSequenceResponseIdxToGrayOut = -1;
	[Space]
	[TextArea(1, 20)]
    public string text;
    [TextArea(1, 20)]
    public string textAlternative;
    [Header("Responses")]
	public DialoguesResponsesDisplayerUI responsesDisplayerPrefab;
	public DialogueResponse[] responses;
    public DialogueResponse[] responsesAlternative;

    [Tooltip("If true, the dialog will continue to the next line automatically when pressing the dialog box, (only works of no responses are present)")]

    [Space(20)]
	public AudioClip audio;
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


[Serializable]
public class DialogueResponse
{
    [TextArea(1, 20)]
    public string response;
    public string responseAlternative;
    public Sprite responseImage;
    public AudioClip responseAudio;
    public DialogueSequenceData dataAfterResponse;
}
