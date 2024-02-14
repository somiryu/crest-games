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

    public override string GetSceneID() => DataIds.boostersAndScapeGame;

    public override void SaveAnalytics()
    {
        var currData = MG_BoostersAndScape_Manager.Instance;
        itemAnalytics = new Dictionary<string, object>();
        itemAnalytics.Add(DataIds.timePlayed, currData.timePlayed);
        itemAnalytics.Add(DataIds.totalClicks, currData.clickRepetitions);
        itemAnalytics.Add(DataIds.lostByCheat, currData.lostByCheat);
        itemAnalytics.Add(DataIds.boostersAndScapeTotalBoostsActivated, currData.boostersActivated);
    }
    public override void ResetCurrentAnalytics()
    {
        base.ResetCurrentAnalytics();
    }
}

