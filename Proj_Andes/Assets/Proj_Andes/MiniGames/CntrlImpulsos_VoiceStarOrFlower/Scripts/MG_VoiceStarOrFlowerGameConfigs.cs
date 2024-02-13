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

    public override string GetSceneID() => DataIds.voiceStarGame;
    
    public override void SaveAnalytics()
    {
        var currAnalyticsDictionary = new Dictionary<string, object>();
        var currAnalytics = MG_VoiceStarOrFlowerManager.Instance.AllRoundsAnalytics;
        for (int i = 0; i < currAnalytics.Count; i++)
        {
            currAnalyticsDictionary.Clear();
            currAnalyticsDictionary.Add(DataIds.voiceStarChallengeType, currAnalytics[i].challengeType);
            currAnalyticsDictionary.Add(DataIds.voiceStarImage, currAnalytics[i].image);
            currAnalyticsDictionary.Add(DataIds.voiceStarSound, currAnalytics[i].audio);
            currAnalyticsDictionary.Add(DataIds.voiceStarRoundResultWin, currAnalytics[i].wonRound);
            currAnalyticsDictionary.Add(DataIds.voiceStartimeToMakeAChoice, currAnalytics[i].timeToMakeAChoice);
            currAnalyticsDictionary.Add(DataIds.voiceStarClickRepetition, currAnalytics[i].clicks);
            currAnalyticsDictionary.Add(DataIds.voiceStarRanOutOfTime, currAnalytics[i].ranOutOfTime);
            UserDataManager.SaveUserAnayticsPerGame(DataIds.voiceStarGame, currAnalyticsDictionary);
        }

    }
    public override void ResetCurrentAnalytics()
    {
        base.ResetCurrentAnalytics();
    }
}

