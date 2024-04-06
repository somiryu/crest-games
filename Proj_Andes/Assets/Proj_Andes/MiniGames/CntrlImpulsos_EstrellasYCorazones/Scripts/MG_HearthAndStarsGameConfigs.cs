using System;
using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;
using UnityEngine.Analytics;

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
		GameID = Guid.NewGuid().ToString();

		var allRoundsAnalytics = MG_HearthsAndStarsManager.Instance.AllRoundsAnalytics;
		var currRoundAnalyticsDic = new Dictionary<string, object>();
        for (int i = 0; i < allRoundsAnalytics.Count; i++)
        {
            var currRound = allRoundsAnalytics[i];
			currRoundAnalyticsDic.Clear();
            currRoundAnalyticsDic.Add(DataIds.GameID, GameID);
            currRoundAnalyticsDic.Add(DataIds.institutionCode, UserDataManager.CurrUser.institutionCode);
            currRoundAnalyticsDic.Add(DataIds.heartsNFlowersFrustration, 0);
			currRoundAnalyticsDic.Add(DataIds.heartsNFlowersSkill, 2);
			currRoundAnalyticsDic.Add(DataIds.heartsNFlowersValid, 1);
			currRoundAnalyticsDic.Add(DataIds.heartsNFlowersTrial, currRound.roundCount);
			currRoundAnalyticsDic.Add(DataIds.heartsNFlowersStimuli, currRound.stimuli);
			currRoundAnalyticsDic.Add(DataIds.heartsNFlowersAnswer, currRound.answer);
			currRoundAnalyticsDic.Add(DataIds.heartsNFlowersCode, currRound.wonRound);
			currRoundAnalyticsDic.Add(DataIds.heartsNFlowersTime, currRound.timeToMakeAChoice);

            UserDataManager.SaveUserAnayticsPerGame(DataIds.heartsAndStarsGame, currRoundAnalyticsDic);
        }

    }

}
