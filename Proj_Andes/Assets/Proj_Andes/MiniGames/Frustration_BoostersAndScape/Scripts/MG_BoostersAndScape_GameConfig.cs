using System.Collections.Generic;
using System;
using UnityEngine;

[CreateAssetMenu(fileName = "BoostersAndScape_GameConfig", menuName = "MiniGames/BoostersAndScape_GameConfig")]
public class MG_BoostersAndScape_GameConfig : GameConfig
{
    public int boostersPerRun;
    public int forcedFails;
    public float boosterTriggerRate;
    public bool forceToFail;
    public bool updateScene;
    public int extraAttemptsBeforeFailing;
    public int coinsOnFailure;
    public static int repetition = 0;

    public override string GetSceneID() => DataIds.boostersAndScapeGame;

    public override void SaveAnalytics()
    {
        repetition ++;
        var currData = MG_BoostersAndScape_Manager.Instance;
		GameID = Guid.NewGuid().ToString();

		itemAnalytics = new Dictionary<string, object>();
        itemAnalytics.Add(DataIds.GameID, GameID);
        itemAnalytics.Add(DataIds.repetition, repetition);
        itemAnalytics.Add(DataIds.timePlayed, currData.timePlayed);
        itemAnalytics.Add(DataIds.totalClicks, currData.clickRepetitions);
        itemAnalytics.Add(DataIds.lostByCheat, currData.lostByCheat);
        itemAnalytics.Add(DataIds.boostersAndScapeTotalBoostsActivated, currData.boostersActivated);

		var newDocID = Guid.NewGuid().ToString();

        UserDataManager.LastCollectionIDStored = DataIds.frustrationGames;
		UserDataManager.LastDocumentIDStored = newDocID;

		UserDataManager.SaveUserAnayticsPerGame(DataIds.frustrationGames, itemAnalytics, newDocID, DataIds.boostersAndScapeGame);

    }
    public override void ResetCurrentAnalytics()
    {
        base.ResetCurrentAnalytics();
    }
}

