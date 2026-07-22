using System;

[Serializable]
public class NarrativeAnalyicsInfo
{
	public NarrativeAnalyticCategory mainCategory;

	public NarrativeAnalticsEmpathyCategories empathySubCategory;

	public NarrativeAnalyticAggSubCategory agressionSubCategory;

	public NarrativeAnalyticConfSubCategory conflictSubCategory;

	public NarrativeAnalyticsFeeling emotionSubCategory;

	public NarrativeAnalticsEmpathyInRelationTo inRelationTo;

	public float customEmotionValue;

	public bool hasCustomID;

	public string codCustomID;

	public string rtaCustomID;

	public string tmCustomID;

	public string BuildID(int narrativeIdx, int questionIdx, NarrativeAnalyticType analyticType)
	{
		return null;
	}

	public string BuildResponse()
	{
		return null;
	}

	public float BuildValue()
	{
		return 0f;
	}
}
