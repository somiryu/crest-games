using System;
using UnityEngine;

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

	public bool hasCustomID;

	public string codCustomID;

	public string rtaCustomID;

	public string tmCustomID;

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
				label += "emocom";
				break;
			case NarrativeAnalyticCategory.EmoBas:
				label += "emobas";
				break;
		}

		label += "_";

		if (inRelationTo != NarrativeAnalticsEmpathyInRelationTo.NONE)
		{
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
			case NarrativeAnalyticType.cod:
				label += "cod";
				break;
		}
		Debug.Log("curr label " + label);
		return label;
	}

	public string BuildResponse()
	{
		if (mainCategory == NarrativeAnalyticCategory.Aggression) return agressionSubCategory.ToString();
		if (mainCategory == NarrativeAnalyticCategory.Conflict) return conflictSubCategory.ToString();
		if (mainCategory == NarrativeAnalyticCategory.Empathy) return empathySubCategory.ToString();
		if (mainCategory == NarrativeAnalyticCategory.EmoBas) return emotionSubCategory.ToString();
		if (mainCategory == NarrativeAnalyticCategory.EmoComp) return emotionSubCategory.ToString();

		return null;
	}

	public float BuildValue()
	{
		Debug.Log("BUILDING VALUE FOR ANALYTICS!");
		if (mainCategory == NarrativeAnalyticCategory.Aggression) return (int)agressionSubCategory;
		if (mainCategory == NarrativeAnalyticCategory.Conflict) return (int)conflictSubCategory;
		if (mainCategory == NarrativeAnalyticCategory.Empathy) return (int)empathySubCategory;
		if (mainCategory == NarrativeAnalyticCategory.EmoBas || mainCategory == NarrativeAnalyticCategory.EmoComp)
		{
			Debug.Log("Custom Emotion Value >> " + customEmotionValue);
			return customEmotionValue;
		}
		return -1;
	}
}
