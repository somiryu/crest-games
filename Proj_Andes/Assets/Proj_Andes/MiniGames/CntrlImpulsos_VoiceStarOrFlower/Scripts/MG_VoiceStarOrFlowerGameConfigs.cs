using System;
using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "VoiceStarOrFlowerGameConfigs", menuName = "MiniGames/VoiceStarOrFlowerGameConfigs")]
public class MG_VoiceStarOrFlowerGameConfigs : GameConfig
{
	public float timePerChoice = 5f;
	public int maxRounds = 10;
	public int initialCoins = 0;
	public int coinsOnCorrectAnswer = 0;
	public int coinsOnWrongAnswer = 0;
    public int finalTutoStepMaxFailuresBeforeSkipping;
    public static bool passedTuto;
    public override string GetSceneID() => DataIds.voiceStarGame;
    
    public override void SaveAnalytics()
    {
        var currAnalyticsDictionary = new Dictionary<string, object>();
        var currAnalytics = MG_VoiceStarOrFlowerManager.Instance.AllRoundsAnalytics;
		GameID = Guid.NewGuid().ToString();
		for (int i = 0; i < currAnalytics.Count; i++)
        {
            currAnalyticsDictionary.Clear();
			currAnalyticsDictionary.Add(DataIds.GameID, GameID);
            currAnalyticsDictionary.Add(DataIds.challengeType, currAnalytics[i].challengeType);
			currAnalyticsDictionary.Add(DataIds.voiceStarImage, currAnalytics[i].image);
            currAnalyticsDictionary.Add(DataIds.voiceStarSound, currAnalytics[i].audio);
            currAnalyticsDictionary.Add(DataIds.won, currAnalytics[i].wonRound);
            currAnalyticsDictionary.Add(DataIds.responseTime, currAnalytics[i].timeToMakeAChoice);
            currAnalyticsDictionary.Add(DataIds.totalClicks, currAnalytics[i].clicks);
            currAnalyticsDictionary.Add(DataIds.lostBecauseOfTime, currAnalytics[i].ranOutOfTime);
            currAnalyticsDictionary.Add(DataIds.voiceStarLostTutorial, passedTuto);

			UserDataManager.SaveUserAnayticsPerGame(DataIds.voiceStarGame, currAnalyticsDictionary);
		}
    }
    public override void ResetCurrentAnalytics()
    {
        base.ResetCurrentAnalytics();
    }
}

