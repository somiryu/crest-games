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
            currAnalyticsDictionary.Add(DataIds.institutionCode, UserDataManager.CurrUser.institutionCode);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerFrustrationMode, 0);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerPassedTuto1, 1);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerPassedTuto2, 1);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerPassedTuto3, 1);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerTrial, currAnalytics[i].roundCount);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerAudibleStimuli, currAnalytics[i].audio);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerVisualStimuli, currAnalytics[i].image);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerAnswer, currAnalytics[i].selection);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerCode, currAnalytics[i].wonRound);
            currAnalyticsDictionary.Add(DataIds.responseTime, currAnalytics[i].timeToMakeAChoice);

			UserDataManager.SaveUserAnayticsPerGame(DataIds.voiceStarGame, currAnalyticsDictionary);
		}
    }
    public override void ResetCurrentAnalytics()
    {
        base.ResetCurrentAnalytics();
    }
}

