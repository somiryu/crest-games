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
        itemAnalytics.Add(DataIds.institutionCode, UserDataManager.CurrInstitutionCode);
        itemAnalytics.Add(DataIds.frustPersTrial , currData.timePlayed);
        itemAnalytics.Add(DataIds.frustPersPresition, currData.clickRepetitions);
        itemAnalytics.Add(DataIds.frustPersBoostClicks, currData.lostByCheat);
        itemAnalytics.Add(DataIds.frustPersFeelAnswer, currData.boostersActivated);
        itemAnalytics.Add(DataIds.frustPersFeelCode, currData.boostersActivated);
        itemAnalytics.Add(DataIds.frustPersFeelTiming, currData.boostersActivated);
        itemAnalytics.Add(DataIds.frustPersWaitClick, currData.boostersActivated);

		var newDocID = Guid.NewGuid().ToString();

        UserDataManager.LastCollectionIDStored = DataIds.frustrationGames;
		UserDataManager.LastDocumentIDStored = newDocID;

		UserDataManager.SaveUserAnayticsPerGame(DataIds.frustrationGames, itemAnalytics, newDocID, DataIds.boostersAndScapeGame);

        MG_MechanicHandGameConfigs.SetPostFrustration();
    }
    public override void ResetCurrentAnalytics()
    {
        base.ResetCurrentAnalytics();
    }
}

