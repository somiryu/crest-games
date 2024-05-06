using System;
using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "MG_SizeRockets_GameConfigs", menuName = "MiniGames/MG_SizeRockets_GameConfigs")]
public class MG_SizeRockets_GameConfigs : GameConfig
{
	public int planetCoins;
	public int shipsPerGame;
	public int shipsAmtLavel1;
    public static int postFrustration = 0;

    public Sprite btnPressed;
    public Sprite btnUnPressed;

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
		GameID = Guid.NewGuid().ToString();
		itemAnalytics = new Dictionary<string, object>();


		for (int i = 0;i < MG_SizeRockets_GameManager.Instance.analyticsPerRound.Length; i++)
		{
			var analytics = MG_SizeRockets_GameManager.Instance.analyticsPerRound[i];
			itemAnalytics.Clear();
			itemAnalytics.Add(DataIds.GameID, GameID);
			itemAnalytics.Add(DataIds.institutionCode, UserDataManager.CurrUser.institutionCode);
			itemAnalytics.Add(DataIds.frustrationMode, postFrustration);
			itemAnalytics.Add(DataIds.tryIndex, analytics.tryIndex);
			itemAnalytics.Add(DataIds.sizeRocketResponseString, analytics.choiceType.ToString());
			itemAnalytics.Add(DataIds.sizeRocketResponseInt, (int) analytics.choiceType);
			itemAnalytics.Add(DataIds.mouseUpCount, analytics.mouseUpCount);
			itemAnalytics.Add(DataIds.starsPerRound, analytics.stars);

			UserDataManager.SaveUserAnayticsPerGame(DataIds.sizeRocketsGame, itemAnalytics);

		}
        Debug.Log("post frust " + postFrustration);
    }
}

[Serializable]
public class SizeRockets_ShipConfig
{
	public SizeRocketsRocketTypes rocketType;
	public float speed;
	public int coinsCapacity;
}
