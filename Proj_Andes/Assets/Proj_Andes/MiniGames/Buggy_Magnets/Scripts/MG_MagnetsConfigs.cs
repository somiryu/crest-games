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
	public int userMagnetRadius;
	public int initialMagnetsCount;
	public int neededEnergyToPick;
	public bool activeCheats;
    [NonSerialized] public Dictionary<string, object> analytics;
    public override Dictionary<string, object> GetAnalytics()
    {
        return genericDictionary;
    }
}
