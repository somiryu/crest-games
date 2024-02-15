using System;
using System.Collections.Generic;
using Tymski;
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

    public override string GetSceneID() => DataIds.mechanicHandGame;
    public override void SaveAnalytics()
    {
        var currData = MG_MechanicHand_GameManger.Instance;
        itemAnalytics = new Dictionary<string, object>();
        itemAnalytics.Add(DataIds.timePlayed, currData.timePlayed);
        itemAnalytics.Add(DataIds.totalClicks, currData.clickRepetitions);
        itemAnalytics.Add(DataIds.lostByCheat, currData.lostByCheat);
        itemAnalytics.Add(DataIds.mechanicHandClawThrows, currData.clawThrows);

		var newDocID = Guid.NewGuid().ToString();

		UserDataManager.LastCollectionIDStored = DataIds.frustrationGames;
		UserDataManager.LastDocumentIDStored = newDocID;

		UserDataManager.SaveUserAnayticsPerGame(DataIds.frustrationGames, itemAnalytics, newDocID, DataIds.mechanicHandGame);

	}
    public override void ResetCurrentAnalytics()
    {
        base.ResetCurrentAnalytics();
    }
}
