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
    public AudioClip responseAudioAlternative;
    public DialogueSequenceData dataAfterResponse;
	public NarrativeAnalyicsInfo analyticInfo;
}

[Serializable]
public class NarrativeAnalyicsInfo
{
    public NarrativeAnalyticCategory mainCategory;
    public NarrativeAnalticsEmpathyCategories empathySubCategory;
    public NarrativeAnalyticAggSubCategory agressionSubCategory;
    public NarrativeAnalyticsFeeling emotionSubCategory;
    public NarrativeAnalticsEmpathyInRelationTo inRelationTo;

    public string BuildID(int narrativeIdx, int questionIdx, NarrativeAnalyticType analyticType)
    {
        var label = "Narr" + narrativeIdx + "_";

        switch (mainCategory)
        {
            case NarrativeAnalyticCategory.Aggression:
                label += "agr";
                break;
            case NarrativeAnalyticCategory.Conflict:
                label += "conf";
                break;
            case NarrativeAnalyticCategory.Empathy:
                label += "emp";
                break;
            case NarrativeAnalyticCategory.EmoComp:
                label += "emocomp";
                break;            
            case NarrativeAnalyticCategory.EmoBas:
                label += "emobas";
                break;
        }
        if (inRelationTo != NarrativeAnalticsEmpathyInRelationTo.NONE)
        {
            label += "_";
            switch (inRelationTo)
            {
                case NarrativeAnalticsEmpathyInRelationTo.self:
                    label += "self";
                    break;                
                case NarrativeAnalticsEmpathyInRelationTo.ami:
                    label += "ami";
                    break;                
                case NarrativeAnalticsEmpathyInRelationTo.ene:
                    label += "ene";
                    break;
            }
            label += questionIdx + "_";
        }
        switch (analyticType)
        {
            case NarrativeAnalyticType.Tm:
                label += "tm";
                break;            
            case NarrativeAnalyticType.Rta:
                label += "rta";
                break;            
            case NarrativeAnalyticType.Val:
                label += "val";
                break;
        }
        return label;
    }

    public string BuildResponse()
    {
        if(mainCategory == NarrativeAnalyticCategory.Aggression || mainCategory == NarrativeAnalyticCategory.Conflict)
        {
            return ((int)agressionSubCategory).ToString();
        }
        if(mainCategory == NarrativeAnalyticCategory.Empathy) return emotionSubCategory.ToString();
        if(mainCategory == NarrativeAnalyticCategory.EmoBas) return emotionSubCategory.ToString();
        if(mainCategory == NarrativeAnalyticCategory.EmoComp) return emotionSubCategory.ToString();

        return null;
    }
}


public enum NarrativeAnalyticCategory
{
    NONE,
    Empathy,
    Aggression,
    Conflict,
    EmoComp,
    EmoBas
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
    Miedo,
    Rabia,
    Orgullo,
    Frustracion,
    Nervios,
    Verguenza,
    Aburrimiento,
    No_se,
}

public enum NarrativeAnalticsEmpathyCategories
{
    NONE,
    Muy_Bien,
    Bien,
    Me_Da_Igual,
    Mal,
    Muy_Mal,
}
public enum NarrativeAnalticsEmpathyInRelationTo
{
    NONE,
    self,
    ami,
    ene
}
public enum NarrativeAnalyticType
{
   Tm,
   Rta,
   Val,
}