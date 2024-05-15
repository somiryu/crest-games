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
        repetition++;
        var currAnalytics = MG_BoostersAndScape_Manager.Instance.currAnalytics;
		GameID = Guid.NewGuid().ToString();
        itemAnalytics = new Dictionary<string, object>();
        UserDataManager.LastDocumentIDsStored = new List<string>();

        for (int i = 0; i < currAnalytics.Count; i++)
        {
            itemAnalytics.Clear();
            var currData = currAnalytics[i];
            itemAnalytics.Add(DataIds.GameID, GameID);
            itemAnalytics.Add(DataIds.frustPersTest, repetition);
            itemAnalytics.Add(DataIds.frustPersTrial, currData.roundCount);
            itemAnalytics.Add(DataIds.frustPersPresition, currData.distanceInBetween);
            itemAnalytics.Add(DataIds.frustPersBoostClicks, currData.clicksToBoost);

            var newDocID = Guid.NewGuid().ToString();

            UserDataManager.LastCollectionIDStored = DataIds.boostersAndScapeGame;
            UserDataManager.LastDocumentIDsStored.Add(newDocID);

            UserDataManager.SaveUserAnayticsPerGame(DataIds.boostersAndScapeGame, itemAnalytics, newDocID);
        }


        MG_MechanicHandGameConfigs.SetPostFrustration();
    }
    public override void ResetCurrentAnalytics()
    {
        base.ResetCurrentAnalytics();
    }
}

