using System;
using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "HearthAndStarsGameConfigs", menuName = "MiniGames/HearthAndStarsGameConfigs")]
public class MG_HearthAndStarsGameConfigs : GameConfig
{
	public float timePerChoice = 5f;
	public int maxRounds = 10;
	public int initialCoins = 0;
	public int coinsOnCorrectAnswer = 0;
	public int coinsOnWrongAnswer = 0;
	[NonSerialized] public Dictionary<string, object> analytics;
    public override Dictionary<string, object> GetAnalytics()
    {
        return genericDictionary;
    }
}
