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
    public int gameIdxInList;
    [NonSerialized] public List<string> challengeOrder = new List<string>();
    [NonSerialized] public List<bool> roundResultWins = new List<bool>();
    [NonSerialized] public List<int> clickRepetitions = new List<int>();
    [NonSerialized] public List<float> timeToMakeAChoice = new List<float>();
    [NonSerialized] public List<bool> ifRanOutOfTime = new List<bool>();

    public override string GetSceneID() => DataIds.heartsAndStarsGame;

    public override void SaveAnalytics()
    {
        itemAnalytics = new Dictionary<string, object>();
        itemAnalytics.Add(DataIds.heartsAndStarsInGameOrder, gameIdxInList);
        itemAnalytics.Add(DataIds.heartsAndStarstimeToMakeAChoice, timeToMakeAChoice);
        itemAnalytics.Add(DataIds.heartsAndStarsRoundResultWins, roundResultWins);
        itemAnalytics.Add(DataIds.heartsAndStarsclickRepetitions, clickRepetitions);
        itemAnalytics.Add(DataIds.heartsAndStarschallengeOrder, challengeOrder);
        itemAnalytics.Add(DataIds.heartsAndStarsifRanOutOfTime, ifRanOutOfTime);
        UserDataManager.SaveUserAnayticsPerGame(DataIds.heartsAndStarsGame, itemAnalytics);
    }
    public override void ResetCurrentAnalytics()
    {
        challengeOrder.Clear();
        clickRepetitions.Clear();
        timeToMakeAChoice.Clear();
        roundResultWins.Clear();
        base.ResetCurrentAnalytics();
    }
}
