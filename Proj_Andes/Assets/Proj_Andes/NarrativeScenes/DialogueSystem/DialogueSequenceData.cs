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
	[Tooltip("If true, the dialog will continue to the next line automatically when pressing the dialog box, (only works if no responses are present)")]
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
    public string analyticChoiceID;
    public string analyticTimeID;
	public DialogueResponse[] responses;
    

    [Space(20)]
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


[Serializable]
public class DialogueResponse
{
    [TextArea(1, 20)]
    public string response;
    [TextArea(1, 20)]
    public string responseAlternative;
    public Sprite responseImage;
    public AudioClip responseAudio;
    public DialogueSequenceData dataAfterResponse;
	public NarrativeAnalyicsInfo analyticInfo;
}

[Serializable]
public class NarrativeAnalyicsInfo
{
    public NarrativeAnalyticCategory mainCategory;
    public NarrativeAnalticsEmphatyCategories empSubCategory;
    public NarrativeAnalyticAggSubCategory aggSubCategory;
    public NarrativeAnalyticsFeeling EmoSubCategory;


    public string BuildID(int narrativeIdx, int questionIdx, bool isTimeLabel)
    {
        var label = "Narr" + narrativeIdx + "_";

        switch (mainCategory)
        {
            case NarrativeAnalyticCategory.Aggression:
                label += "Agg";
                break;
            case NarrativeAnalyticCategory.Emo:
                label += "Emo";
                break;
            case NarrativeAnalyticCategory.Conflict:
                label += "conf";
                break;
            case NarrativeAnalyticCategory.Empathy:
                label += "Emp";
                break;
        }

        label += questionIdx + "_";

        label += isTimeLabel ? "Time" : "Response";

        return label;
    }

    public string buildResponse()
    {
        if(mainCategory == NarrativeAnalyticCategory.Aggression || mainCategory == NarrativeAnalyticCategory.Conflict)
        {
            return aggSubCategory.ToString();
        }
        if(mainCategory == NarrativeAnalyticCategory.Empathy) return empSubCategory.ToString();
        if(mainCategory == NarrativeAnalyticCategory.Emo) return EmoSubCategory.ToString();

        return null;
    }
}


public enum NarrativeAnalyticCategory
{
    NONE,
    Empathy,
    Aggression,
    Conflict,
    Emo,
}

public enum NarrativeAnalyticAggSubCategory
{
    NONE,
    Aggresive,
    Evasive,
    Assertive,
}

public enum NarrativeAnalyticsFeeling
{
    NONE,
    Tristeza,
    Alegría,
    Tranquilidad,
    Miedo,
    Rabia,
    No_se,
    Orgullo,
    Frustracion,
    Nervios,
    Verguenza,
    Aburrimiento,

}

public enum NarrativeAnalticsEmphatyCategories
{
    NONE,
    Muy_Bien,
    Bien,
    Me_Da_Igual,
    Mal,
    Muy_Mal,
}