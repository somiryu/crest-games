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

    [NonSerialized] public List<float> timeToMakeAChoice = new List<float>();
    [NonSerialized] public int totalAttemptsToBoost;
    [NonSerialized] public List<bool> roundResultWins = new List<bool>();
    [NonSerialized] public float totalGameTime;

    public override string GetSceneID() => DataIds.boostersAndScapeGame;

    public override void SaveAnalytics()
    {
        itemAnalytics = new Dictionary<string, object>();
        itemAnalytics.Add(DataIds.boostersAndScapetotalAttemptsToBoost, totalAttemptsToBoost);
        itemAnalytics.Add(DataIds.boostersAndScapeTotalGametime, totalGameTime);

        UserDataManager.SaveUserAnayticsPerGame(DataIds.boostersAndScapeGame, itemAnalytics);
    }
    public override void ResetCurrentAnalytics()
    {
        timeToMakeAChoice.Clear();
        roundResultWins.Clear();
        totalGameTime = 0;
        base.ResetCurrentAnalytics();
    }
}

