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
    [NonSerialized] public List<float> timeToMakeAChoice = new List<float>();
    [NonSerialized] public List<bool> roundResultWins = new List<bool>();
    public override string GetSceneID() => DataIds.heartsAndStarsGame;

    public override void SaveAnalytics()
    {
        itemAnalytics = new Dictionary<string, object>();
        itemAnalytics.Add(DataIds.heartsAndStarstimeToMakeAChoice, timeToMakeAChoice);
        itemAnalytics.Add(DataIds.heartsAndStarsRoundResultWins, roundResultWins);
        Debug.Log(timeToMakeAChoice.Count + " handstarts " + roundResultWins.Count);
        UserDataManager.SaveUserAnayticsPerGame(DataIds.heartsAndStarsGame, itemAnalytics);
    }
    public override void ResetCurrentAnalytics()
    {
        timeToMakeAChoice.Clear();
        roundResultWins.Clear();
        base.ResetCurrentAnalytics();
    }
}
