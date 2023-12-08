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
    [Space]
    public DialogueSequenceData changeToSequence;
	[Space]
	[TextArea(1, 20)]
    public string text;
	[Header("Responses")]
	public DialoguesResponsesDisplayerUI responsesDisplayerPrefab;
	public DialogueResponse[] responses;
    [Tooltip("If true, the dialog will continue to the next line automatically when pressing the dialog box, (only works of no responses are present)")]

    [Space(20)]
	public AudioClip audio;
	[Header("Anims")]
    public TimelineAsset EnterAnim;
    public TimelineAsset IdleAnim;
    public TimelineAsset ExitAnim;
}


[Serializable]
public class DialogueResponse
{
    [TextArea(1, 20)]
    public string response;
    public Sprite responseImage;
    public AudioClip responseAudio;
    public bool changeSequence;
    public DialogueSequenceData dataAfterResponse;
}
