using System;
using System.Collections;
using System.Collections.Generic;
using Tymski;
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
	[NonSerialized] public int energyCaptured;

	public DifficultyModificatorFloat timeBetweenSpawnsPerDifficultLevel;
    
	[HideInInspector] [NonSerialized] public int coinsCollected;

    public override void SaveAnalytics()
    {
        itemAnalytics = new Dictionary<string, object>();
        itemAnalytics.Add(DataIds.magnetsEneryPicked, energyCaptured);
        SaveCoins(coinsCollected);
    }
    public override void ResetCurrentAnalytics()
    {
		energyCaptured = 0;
        base.ResetCurrentAnalytics();
    }
}
