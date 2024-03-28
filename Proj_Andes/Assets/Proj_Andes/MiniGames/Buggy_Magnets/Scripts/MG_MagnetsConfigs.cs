using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MG_MagnetsConfigs", menuName = "MiniGames/MG_MagnetsConfigs")]
public class MG_MagnetsConfigs : GameConfig
{
	public float timeBetweenSpawns;
	public int maxSpawnsOnScreen;
	public float userMagnetRadius;
	public int initialMagnetsCount;
	public int neededEnergyToPick;
	public bool activeCheats;
	public float energyItemsLifeTime = 5;
	public int itemsAmountToSpawn = 3;
    public int coinsOnFailure;
    public int startFaillingAfterAttemps = 5;

	public DifficultyModificatorFloat timeBetweenSpawnsPerDifficultLevel;
    
	[HideInInspector] [NonSerialized] public int coinsCollected;

    public override string GetSceneID() => DataIds.magnetsGame;
    public override void SaveAnalytics()
    {
        var currData = MG_MagnetsGameManager.Instance;
		GameID = Guid.NewGuid().ToString();

		itemAnalytics = new Dictionary<string, object>(); 
        itemAnalytics.Add(DataIds.GameID, GameID);
        itemAnalytics.Add(DataIds.timePlayed, currData.timePlayed);
		itemAnalytics.Add(DataIds.totalClicks, currData.clickRepetitions);
        itemAnalytics.Add(DataIds.lostByCheat, currData.lostByCheat);
        itemAnalytics.Add(DataIds.magnetsEneryPicked, currData.magnetsCollected);

        var newDocID = Guid.NewGuid().ToString();

        UserDataManager.LastCollectionIDStored = DataIds.frustrationGames;
        UserDataManager.LastDocumentIDStored = newDocID;

        UserDataManager.SaveUserAnayticsPerGame(DataIds.frustrationGames, itemAnalytics, newDocID, DataIds.magnetsGame);

        SaveCoins(coinsCollected);
    }
    public override void ResetCurrentAnalytics()
    {
        base.ResetCurrentAnalytics();
    }
}
