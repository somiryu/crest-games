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
    public float tutorialRideDuration;
    public int starsAmount;
    public int tutoStarsAmount;
    public float minTurboTime;
    [HideInInspector][NonSerialized] public int coinsCollected;

    [NonSerialized] public int turboUsedTimes;
    [NonSerialized] public float totalTurboTime;

    public static int postFrustration = 0;
    public override string GetSceneID() => DataIds.turboRocketGame;

    public override void SaveAnalytics()
    {
        itemAnalytics = new Dictionary<string, object>();
		GameID = Guid.NewGuid().ToString();

		itemAnalytics.Add(DataIds.GameID, GameID);
		itemAnalytics.Add(DataIds.institutionCode, UserDataManager.CurrInstitutionCode);
		itemAnalytics.Add(DataIds.turboStarsFrustrationMode, postFrustration);
        itemAnalytics.Add(DataIds.turboStarsStars, coinsCollected);
        itemAnalytics.Add(DataIds.turboStarsTurboUses, turboUsedTimes);
        itemAnalytics.Add(DataIds.turboStarsTurboTime, totalTurboTime);
        var timePlayed = GeneralGameAnalyticsManager.Instance.analytics.timePlayed;
        itemAnalytics.Add(DataIds.turboStarsTime, timePlayed);

        SaveCoins(coinsCollected);

		UserDataManager.SaveUserAnayticsPerGame(DataIds.turboRocketGame,itemAnalytics);
    }

    public override void ResetCurrentAnalytics()
    {
        turboUsedTimes = 0;
        totalTurboTime = 0;
        coinsCollected = 0;
        base.ResetCurrentAnalytics();
    }
}
