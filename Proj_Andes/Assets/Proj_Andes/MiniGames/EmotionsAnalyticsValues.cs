using System;
using System.Collections.Generic;

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
