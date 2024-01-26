using System;
using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "Gratification_TurboRocket_GameConfig", menuName = "MiniGames/Gratification_TurboRocket_GameConfig")]

public class Gratification_TurboRocket_GameConfig : GameConfig
{
    public float regularSpeed;
    public float turboSpeed;
    public float accelerationSpeed;
    public float regularRideDuration;
    public int starsAmount;
    [HideInInspector][NonSerialized] public int coinsCollected;

    [NonSerialized] public int turboUsedTimes;
    [NonSerialized] public float totalRideTime;
    public override string GetSceneID() => DataIds.turboRocketGame;

    public override void SaveAnalytics()
    {
        itemAnalytics = new Dictionary<string, object>();
        itemAnalytics.Add(DataIds.turboRocketturboUsedTimes, turboUsedTimes);
        itemAnalytics.Add(DataIds.turboRockettotalRideTime, totalRideTime);
        itemAnalytics.Add(DataIds.turboRocketCoinsCollected, coinsCollected);

        SaveCoins(coinsCollected);

        UserDataManager.SaveUserAnayticsPerGame(DataIds.turboRocketGame, itemAnalytics);

    }
    public override void ResetCurrentAnalytics()
    {
        turboUsedTimes = 0;
        totalRideTime = 0;
        coinsCollected = 0;
        base.ResetCurrentAnalytics();
    }
}
