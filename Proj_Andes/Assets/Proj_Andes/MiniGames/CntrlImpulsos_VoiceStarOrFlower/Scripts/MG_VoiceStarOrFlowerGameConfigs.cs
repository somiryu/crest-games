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
	[NonSerialized] List<float> timeToMakeAChoice = new List<float>();
    [NonSerialized] List<bool> roundResultWins = new List<bool>();
    [NonSerialized] float totalGameTime;
    public override void SaveAnalytics()
    {
        itemAnalytics = new Dictionary<string, object>();
        itemAnalytics.Add(DataIds.voiceStarTotalGametime, totalGameTime);
        itemAnalytics.Add(DataIds.voiceStartimeToMakeAChoice, timeToMakeAChoice);
        itemAnalytics.Add(DataIds.voiceStarRoundResultWins, roundResultWins);
    }
    public void GetPlaytimeAnalytics(List<float> timePerChoice, List<bool> roundResults, float totalTime)
    {
        timeToMakeAChoice = timePerChoice;
        roundResultWins = roundResults;
        totalGameTime = totalTime;
        SaveAnalytics();
    }
}

