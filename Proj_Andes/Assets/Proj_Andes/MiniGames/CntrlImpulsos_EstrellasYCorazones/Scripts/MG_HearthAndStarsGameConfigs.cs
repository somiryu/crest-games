using System;
using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;
using UnityEngine.Analytics;

[CreateAssetMenu(fileName = "HearthAndStarsGameConfigs", menuName = "MiniGames/HearthAndStarsGameConfigs")]
public class MG_HearthAndStarsGameConfigs : GameConfig
{
	/// <summary>
	/// Is this the last game of the batch? (a batch right now is the sequence of only hearths, only stars, and lastly mixed)
	/// </summary>
	public bool isLastHearthAndStarsGameOnBatch = false;
	public float intermidiateRoundHold;
	public static string CurrHearthAndStarsGameID;
	public static GeneralGameAnalytics GlobalGeneralGameAnalytics;
	public static int passedTuto = 0;

	public float timePerChoice = 5f;
	public int maxRounds = 8;
	public int maxRoundsOnMix = 8;
	public int initialCoins = 0;
	public int coinsOnCorrectAnswer = 0;
	public int coinsOnWrongAnswer = 0;
	public HeartsAndFlowersGameType gameType;
	public static int postFrustration = 0;
	public override string GetSceneID() => DataIds.heartsAndStarsGame;

    public override SimpleGameSequenceItem GetNextItem()
    {
        var currItem = base.GetNextItem();
        if (currItem != null) MG_HearthsAndStarsManager.currGameType = gameType;
        return currItem;
    }


    public override void SaveAnalytics()
	{
		if (string.IsNullOrEmpty(CurrHearthAndStarsGameID))
		{
			CurrHearthAndStarsGameID = Guid.NewGuid().ToString();
			GlobalGeneralGameAnalytics = new GeneralGameAnalytics();
		}

		GameID = CurrHearthAndStarsGameID;
		shouldTryToSaveGeneralAnalytics = isLastHearthAndStarsGameOnBatch;

		Debug.Log("Will save general game analytics: " + shouldTryToSaveGeneralAnalytics);

		if (!shouldTryToSaveGeneralAnalytics)
		{
			//If we are not going to save the general analytics, then add them to global, so that we can save them later on
			GlobalGeneralGameAnalytics.CopyFrom(GeneralGameAnalyticsManager.Instance.analytics);
		}

		var allRoundsAnalytics = MG_HearthsAndStarsManager.Instance.AllRoundsAnalytics;
		var currRoundAnalyticsDic = new Dictionary<string, object>();
		for (int i = 0; i < allRoundsAnalytics.Count; i++)
		{
			var currRound = allRoundsAnalytics[i];
			currRoundAnalyticsDic.Clear();
            currRoundAnalyticsDic.Add(DataIds.GameID, GameID);
            currRoundAnalyticsDic.Add(DataIds.heartsNFlowersFrustration, postFrustration);
			currRoundAnalyticsDic.Add(DataIds.heartsNFlowersSkill, (int)gameType);
			currRoundAnalyticsDic.Add(DataIds.heartsNFlowersTest, (int)gameType);
			currRoundAnalyticsDic.Add(DataIds.heartsNFlowersValid, passedTuto);
			currRoundAnalyticsDic.Add(DataIds.heartsNFlowersTrial, currRound.roundCount);
			currRoundAnalyticsDic.Add(DataIds.heartsNFlowersStimuli, currRound.stimuli);
			currRoundAnalyticsDic.Add(DataIds.heartsNFlowersAnswer, currRound.answer);
			currRoundAnalyticsDic.Add(DataIds.heartsNFlowersCode, currRound.wonRound);
			currRoundAnalyticsDic.Add(DataIds.heartsNFlowersTime, currRound.timeToMakeAChoice);

			UserDataManager.SaveUserAnayticsPerGame(DataIds.heartsAndStarsGame, currRoundAnalyticsDic);
		}
        Debug.Log("post frust " + postFrustration);
    }

    public override void AferGeneralAnalyticsSaved()
	{
		//We clean the game ID so that different "batches" of hearth and stars games happend, they do have different IDs
		if (!isLastHearthAndStarsGameOnBatch) return;

		CurrHearthAndStarsGameID = null;
		GlobalGeneralGameAnalytics = new GeneralGameAnalytics();
	}

	public override void OnReset()
	{
		Debug.Log("Resetted Hearth and stars game ID");
		CurrHearthAndStarsGameID = null;
		GlobalGeneralGameAnalytics = new GeneralGameAnalytics();
	}

}
public enum HeartsAndFlowersGameType
{
	Hearts,
	Flowers,
	Mixed
}
