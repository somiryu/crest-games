using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GeneralGameAnalyticsManager : MonoBehaviour
{
	private static GeneralGameAnalyticsManager instance;
	public static GeneralGameAnalyticsManager Instance => instance;

	public GeneralGameAnalytics analytics = new GeneralGameAnalytics();

	public float clickTimer;

	private void Awake()
	{
		if(instance != null && instance != this) DestroyImmediate(instance);
		instance = this;
		analytics = new GeneralGameAnalytics();
	}

	public void Init(string gameType)
	{
		if(analytics == null) analytics = new GeneralGameAnalytics();
		analytics.gameType = gameType;
	}

	public void Init(string gameType, GeneralGameAnalytics previousAnalytics)
	{
		Init(gameType);
		if (previousAnalytics == null) return;
		analytics.lastFrustrationLevel = previousAnalytics.lastFrustrationLevel;
		analytics.timePlayed = previousAnalytics.timePlayed;
		analytics.timesBetweenClicks = previousAnalytics.timesBetweenClicks;
		analytics.timePlayed = previousAnalytics.timePlayed;
		analytics.achievements = previousAnalytics.achievements;
		analytics.losses = previousAnalytics.losses;
		analytics.clicks = previousAnalytics.clicks;
		analytics.timesBetweenClicks = new List<float>();
		for (int i = 0; i < previousAnalytics.timesBetweenClicks.Count; i++)
		{
			analytics.timesBetweenClicks.Add(previousAnalytics.timesBetweenClicks[i]);
		}
	}

	private void Update()
	{
		analytics.timePlayed += Time.deltaTime;
		HandleMouseClicks();
	}

	void HandleMouseClicks()
	{
		clickTimer += Time.deltaTime;
		if (!Input.GetMouseButtonDown(0)) return;
		analytics.clicks++;
		analytics.timesBetweenClicks.Add(clickTimer);
		clickTimer = 0;
	}

	public static void RegisterWin() => instance.analytics.achievements++;
	public static void RegisterLose() => instance.analytics.losses++;


	public Dictionary<string, object> GetAnalytics()
	{
		if (string.IsNullOrEmpty(analytics.gameType)) return null;

		var analyticsDic = new Dictionary<string, object>();

		analyticsDic.Add(DataIds.GameOrderInSequence, GameSequencesList.Instance.goToGameGroupIdx);
		analyticsDic.Add(DataIds.GameType, analytics.gameType);
		analyticsDic.Add(DataIds.timePlayed, analytics.timePlayed);
		analyticsDic.Add(DataIds.wins, analytics.achievements);
		analyticsDic.Add(DataIds.losses, analytics.losses);
		analyticsDic.Add(DataIds.clicks, analytics.clicks);

		var averageClickTime = 0f;
		for (int i = 0; i < analytics.timesBetweenClicks.Count; i++)
		{
			averageClickTime += analytics.timesBetweenClicks[i];
		}
		averageClickTime/= analytics.timesBetweenClicks.Count;

		analyticsDic.Add(DataIds.averageClickTime, averageClickTime);

		analytics.lastFrustrationLevel = FrustrationTermometer.LastFrustrationLevelPicked;
		FrustrationTermometer.LastFrustrationLevelPicked = FrustrationLevel.NONE;

		analyticsDic.Add(DataIds.lastFrustrationLevel, (int) analytics.lastFrustrationLevel.GetAnalyticValue());


		return analyticsDic;

	}

}



public class GeneralGameAnalytics
{
	public string gameType;
	public float timePlayed;
	public int achievements;
	public int losses;
	public int clicks;
	public List<float> timesBetweenClicks = new List<float>();
	public FrustrationLevel lastFrustrationLevel = FrustrationLevel.NONE;

	public float GetAverageClickTime()
	{
		var averageClickTime = 0f;
		for (int i = 0; i < timesBetweenClicks.Count; i++)
		{
			averageClickTime += timesBetweenClicks[i];
		}
		averageClickTime /= timesBetweenClicks.Count;
		return averageClickTime;
	}

	public void CopyFrom(GeneralGameAnalytics other)
	{
		if(other == null) return;
		gameType = other.gameType;
		timePlayed = other.timePlayed;
		achievements = other.achievements;
		losses = other.losses;
		clicks = other.clicks;
		lastFrustrationLevel = other.lastFrustrationLevel;
		timesBetweenClicks = new List<float>();
		for (int i = 0; i < other.timesBetweenClicks.Count; i++)
		{
			timesBetweenClicks.Add(other.timesBetweenClicks[i]);
		}
	}


}

public enum GameModeAboutFrustration
{
	baseline,
	Frustration,
	Post
}