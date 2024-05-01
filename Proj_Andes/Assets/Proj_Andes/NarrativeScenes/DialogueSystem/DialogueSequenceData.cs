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
    public NarrativeAnalyticConfSubCategory conflictSubCategory;
    public NarrativeAnalyticsFeeling emotionSubCategory;
    public NarrativeAnalticsEmpathyInRelationTo inRelationTo;
    public float customEmotionValue = 0;

    public string BuildID(int narrativeIdx, int questionIdx, NarrativeAnalyticType analyticType)
    {
        var label = "m" + (narrativeIdx + 1) + "_";

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
                    label += "self_";
                    break;                
                case NarrativeAnalticsEmpathyInRelationTo.ami:
                    label += "ami_";
                    break;                
                case NarrativeAnalticsEmpathyInRelationTo.ene:
                    label += "ene_";
                    break;

            }
        }
		label += (questionIdx + 1) + "_";

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
        Debug.Log("curr label " + label);
        return label;
    }

    public string BuildResponse()
    {
        if(mainCategory == NarrativeAnalyticCategory.Aggression) return agressionSubCategory.ToString();
        if(mainCategory == NarrativeAnalyticCategory.Conflict) return conflictSubCategory.ToString();
        if(mainCategory == NarrativeAnalyticCategory.Empathy) return empathySubCategory.ToString();
        if(mainCategory == NarrativeAnalyticCategory.EmoBas) return emotionSubCategory.ToString();
        if(mainCategory == NarrativeAnalyticCategory.EmoComp) return emotionSubCategory.ToString();

        return null;
    }

    public float BuildValue()
    {
        if (mainCategory == NarrativeAnalyticCategory.Aggression || mainCategory == NarrativeAnalyticCategory.Conflict)
        {
            return ((int)agressionSubCategory);
        }
        if (mainCategory == NarrativeAnalyticCategory.Empathy) return (int)empathySubCategory;
        if(mainCategory == NarrativeAnalyticCategory.EmoBas || mainCategory == NarrativeAnalyticCategory.EmoComp)
        {
            return customEmotionValue;
        }
        return -1;
    }
}

[Serializable]
public class EmotionsAnalyticsValues
{
    public List<EmotionAnalyticValue> customValues;

    public float GetValueForEmotion(NarrativeAnalyticsFeeling type)
    {
        for (int i = 0; i < customValues.Count; i++)
        {
            if (customValues[i].emotionType == type) return customValues[i].value;
        }
        return 0;
    }
}

[Serializable]
public class EmotionAnalyticValue
{
    public NarrativeAnalyticsFeeling emotionType;
    public float value;
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
    NONE = 99,
    Agresiva = 0,
    Evitativa = 1,
    Asertiva = 2,
}

public enum NarrativeAnalyticConfSubCategory
{
	NONE = 99,
	imponer = 0,
	Ceder = 1,
	Negociar = 2,
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
    NONE = 99,
    Muy_Bien = 0,
    Bien = 1,
    Me_Da_Igual = 2,
    Mal = 3,
    Muy_Mal = 4,
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