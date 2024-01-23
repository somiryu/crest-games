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
    public override void SaveAnalytics()
    {
        itemAnalytics = new Dictionary<string, object>();
        itemAnalytics.Add(DataIds.voiceStarTotalGametime, totalGameTime);
        itemAnalytics.Add(DataIds.voiceStartimeToMakeAChoice, timeToMakeAChoice);
        itemAnalytics.Add(DataIds.voiceStarRoundResultWins, roundResultWins);

        FireStoreManager.AddData(DataIds.voiceStarGame, UserDataManager.CurrUser.id, itemAnalytics);
        Debug.Log("save in firestore");
    }
    public override void ResetCurrentAnalytics()
    {
        timeToMakeAChoice.Clear();
        roundResultWins.Clear();
        totalGameTime = 0;
        base.ResetCurrentAnalytics();
    }
}

