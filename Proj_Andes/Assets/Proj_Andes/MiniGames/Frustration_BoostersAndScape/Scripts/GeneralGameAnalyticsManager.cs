using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GeneralGameAnalyticsManager : MonoBehaviour
{
	private static GeneralGameAnalyticsManager instance;
	public static GeneralGameAnalyticsManager Instance => instance;

	public GeneralGameAnalytics analytics;

	public float clickTimer;

	private void Awake()
	{
		if(instance != null && instance != this) DestroyImmediate(instance);
		instance = this;
	}

	private void Start()
	{
		analytics = new GeneralGameAnalytics();
	}

	public void Init(string gameType)
	{
		analytics.gameType = gameType;
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

		var gameMode = GameModeAboutFrustration.baseline;
		var currGameIdx = GameSequencesList.Instance.goToGameGroupIdx;
		if (currGameIdx > GameSequencesList.firstFrustrationScreenIdx
			&& currGameIdx < GameSequencesList.LastFrustrationScreenIdx) gameMode = GameModeAboutFrustration.Frustration;
		else if (currGameIdx > GameSequencesList.LastFrustrationScreenIdx) gameMode = GameModeAboutFrustration.Post;


		analyticsDic.Add(DataIds.GameModeAboutAnalytics, gameMode);
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

		analyticsDic.Add(DataIds.lastFrustrationLevel, (int) analytics.lastFrustrationLevel);


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


}

public enum GameModeAboutFrustration
{
	baseline,
	Frustration,
	Post
}