using System;
using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "MG_SizeRockets_GameConfigs", menuName = "MiniGames/MG_SizeRockets_GameConfigs")]
public class MG_SizeRockets_GameConfigs : GameConfig
{
	public int minCoinsToGive;
	public int maxCoinsToGive;

	public int planetsAmountToGenerate;
	public int shipsPerGame;

	public SizeRockets_ShipConfig[] shipsConfigs;

    public SizeRockets_ShipConfig GetShipConfig(SizeRocketsRocketTypes shipType)
	{
		for (int i = 0; i < shipsConfigs.Length; i++)
		{
			if (shipsConfigs[i].rocketType == shipType) return shipsConfigs[i];
		}
		return null;
	}

}

[Serializable]
public class SizeRockets_ShipConfig
{
	public SizeRocketsRocketTypes rocketType;
	public float speed;
	public int coinsCapacity;
}
