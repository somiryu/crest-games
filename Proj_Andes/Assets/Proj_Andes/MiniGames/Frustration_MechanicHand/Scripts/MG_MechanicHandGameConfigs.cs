using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MG_MechanicHandGameConfigs", menuName = "MiniGames/MG_MechanicHandGameConfigs")]
public class MG_MechanicHandGameConfigs : GameConfig
{
	public int asteroidsPerRound = 5;
	public float asteroidsSizeLoseAmountPerRound = 0.2f;
	public int playerLifes = 5;
	public float hookRotationSpeed = 1;
	public float hookMoveSpeed = 1;
	[Range(0f,1f)]
	public float percentageNeededToWin = 0.8f;
	public bool activeCheats;
	public static int repetition = 0;

    public override string GetSceneID() => DataIds.mechanicHandGame;
    public override void SaveAnalytics()
    {
		repetition++;
        var currData = MG_MechanicHand_GameManger.Instance;
		GameID = Guid.NewGuid().ToString();

		itemAnalytics = new Dictionary<string, object>();
        itemAnalytics.Add(DataIds.GameID, GameID);
        itemAnalytics.Add(DataIds.institutionCode, UserDataManager.CurrInstitutionCode);
        itemAnalytics.Add(DataIds.mechHandTrial, currData.timePlayed);
		itemAnalytics.Add(DataIds.mechHandThrown, currData.clickRepetitions);
        itemAnalytics.Add(DataIds.mechHandPresition, currData.lostByCheat);
        itemAnalytics.Add(DataIds.mechHandFeelAnswer, currData.clawThrows);
        itemAnalytics.Add(DataIds.mechHandFeelCode, currData.clawThrows);
        itemAnalytics.Add(DataIds.mechHandFeelTiming, currData.clawThrows);
        itemAnalytics.Add(DataIds.mechHandWaitClick, currData.clawThrows);

		var newDocID = Guid.NewGuid().ToString();

		UserDataManager.LastCollectionIDStored = DataIds.frustrationGames;
		UserDataManager.LastDocumentIDStored = newDocID;

		UserDataManager.SaveUserAnayticsPerGame(DataIds.frustrationGames, itemAnalytics, newDocID, DataIds.mechanicHandGame);
		SetPostFrustration();
	}

	public static void SetPostFrustration()
	{
		MG_SizeRockets_GameConfigs.postFrustration = 1;
		MG_VoiceStarOrFlowerGameConfigs.postFrustration = 1;
		MG_HearthAndStarsGameConfigs.postFrustration = 1;
		Gratification_TurboRocket_GameConfig.postFrustration = 1;
	}
    public override void ResetCurrentAnalytics()
    {
        base.ResetCurrentAnalytics();
    }
}
