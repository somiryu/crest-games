using System;
using System.Collections.Generic;

[Serializable]
public class EmotionsAnalyticsValues
{
	public List<EmotionAnalyticValue> customValues;

	public float GetValueForEmotion(NarrativeAnalyticsFeeling type)
	{
		return 0f;
	}
}
