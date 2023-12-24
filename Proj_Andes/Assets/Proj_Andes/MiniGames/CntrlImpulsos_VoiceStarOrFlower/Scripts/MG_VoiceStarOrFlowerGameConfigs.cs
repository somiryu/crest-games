using System;
using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "VoiceStarOrFlowerGameConfigs", menuName = "MiniGames/VoiceStarOrFlowerGameConfigs")]
public class MG_VoiceStarOrFlowerGameConfigs : GameConfig
{
	public float timePerChoice = 5f;
	public int maxRounds = 10;
	public int initialCoins = 0;
	public int coinsOnCorrectAnswer = 0;
	public int coinsOnWrongAnswer = 0;
	[NonSerialized] public List<float> timeToMakeAChoice = new List<float>();
    [NonSerialized] public List<bool> roundResultWins = new List<bool>();
    [NonSerialized] public float totalGameTime;
    Dictionary<string, object> analytics;

    public override Dictionary<string, object> GetAnalytics()
    {
        analytics = new Dictionary<string, object>();
        analytics.Add(DataIds.voiceStarTotalGametime, totalGameTime);
        analytics.Add(DataIds.voiceStartimeToMakeAChoice, timeToMakeAChoice);
        analytics.Add(DataIds.voiceStarRoundResultWins, roundResultWins);
        return analytics;
    }
}

