using System;
using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "HearthAndStarsGameConfigs", menuName = "MiniGames/HearthAndStarsGameConfigs")]
public class MG_HearthAndStarsGameConfigs : GameConfig
{
	public float timePerChoice = 5f;
	public int maxRounds = 10;
	public int initialCoins = 0;
	public int coinsOnCorrectAnswer = 0;
	public int coinsOnWrongAnswer = 0;

    public override string GetSceneID() => DataIds.heartsAndStarsGame;

    public override void SaveAnalytics()
    {
        var allRoundsAnalytics = MG_HearthsAndStarsManager.Instance.AllRoundsAnalytics;
		var currRoundAnalyticsDic = new Dictionary<string, object>();

        for (int i = 0; i < allRoundsAnalytics.Count; i++)
        {
            var currRound = allRoundsAnalytics[i];
			currRoundAnalyticsDic.Clear();
			currRoundAnalyticsDic.Add(DataIds.challengeType, currRound.challengeOrder);
			currRoundAnalyticsDic.Add(DataIds.won, currRound.wonRound);
			currRoundAnalyticsDic.Add(DataIds.responseTime, currRound.timeToMakeAChoice);
			currRoundAnalyticsDic.Add(DataIds.totalClicks, currRound.clicks);
			currRoundAnalyticsDic.Add(DataIds.lostBecauseOfTime, currRound.ranOutOfTime);

			UserDataManager.SaveUserAnayticsPerGame(DataIds.heartsAndStarsGame, currRoundAnalyticsDic, GameID);
            Debug.Log("hearts start config " + GameID);
        }

    }

}
