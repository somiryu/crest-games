using System;
using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "MG_SizeRockets_GameConfigs", menuName = "MiniGames/MG_SizeRockets_GameConfigs")]
public class MG_SizeRockets_GameConfigs : GameConfig
{
	public int closePlanetCoins;
	public int middlePlanetCoins;
	public int FarPlanetCoins;

	public int shipsPerGame;

    [HideInInspector][NonSerialized] public int coinsCollected;

    public SizeRockets_ShipConfig[] shipsConfigs;
    public override string GetSceneID() => DataIds.sizeRocketsGame;


    public SizeRockets_ShipConfig GetShipConfig(SizeRocketsRocketTypes shipType)
	{
		for (int i = 0; i < shipsConfigs.Length; i++)
		{
			if (shipsConfigs[i].rocketType == shipType) return shipsConfigs[i];
		}
		return null;
	}
    public override void SaveAnalytics()
    {
		var analytics = MG_SizeRockets_GameManager.Instance.currAnalytics;
        itemAnalytics = new Dictionary<string, object>();
		itemAnalytics.Add(DataIds.timePlayed, analytics.timePlayed);
		itemAnalytics.Add(DataIds.sizeRocketsBigShips, analytics.bigShipsCount);
		itemAnalytics.Add(DataIds.sizeRocketsMidShips, analytics.mediumShipsCount);
		itemAnalytics.Add(DataIds.sizeRocketsSmallShips, analytics.smallShipsCount);
		itemAnalytics.Add(DataIds.stars, analytics.stars);
		itemAnalytics.Add(DataIds.sizeRocketsClosePlanets, analytics.closePlanets);
		itemAnalytics.Add(DataIds.sizeRocketsMidPlanets, analytics.midPlanets);
		itemAnalytics.Add(DataIds.sizeRocketsFarPlanets, analytics.farPlanets);
		itemAnalytics.Add(DataIds.averageClickTime, analytics.averageClick);

        UserDataManager.SaveUserAnayticsPerGame(DataIds.sizeRocketsGame, itemAnalytics);

    }
}

[Serializable]
public class SizeRockets_ShipConfig
{
	public SizeRocketsRocketTypes rocketType;
	public float speed;
	public int coinsCapacity;
}
