using System;
using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "MG_FightTheAlienGameConfigs", menuName = "MiniGames/MG_FightTheAlienGameConfigs")]
public class MG_FightTheAlienGameConfigs : GameConfig
{
	public float timePerChoice = 5f;
	public int PlayerHealth = 10;
	public int EnemyHealth = 10;
	public int initialCoins = 0;
	public int coinsOnCorrectAnswer = 0;
	public int coinsOnWrongAnswer = 0;

	public int playerHealthLostOnWrongAnswer = -1;
	public int EnemyHealthLostOnRightAnswer = -1;

    [NonSerialized] public List<float> timeToMakeAChoice = new List<float>();
    [NonSerialized] public List<bool> roundResultWins = new List<bool>();
    [NonSerialized] public float totalGameTime;

    public override string GetSceneID() => DataIds.fightTheAlienGame;

    public override void SaveAnalytics()
    {
        itemAnalytics = new Dictionary<string, object>();
        itemAnalytics.Add(DataIds.fightTheAlienTotalGametime, totalGameTime);

        UserDataManager.SaveUserAnayticsPerGame(DataIds.fightTheAlienGame, itemAnalytics);

        Debug.Log(timeToMakeAChoice.Count + "fight " + roundResultWins.Count + " " + totalGameTime);
    }
    public override void ResetCurrentAnalytics()
    {
        timeToMakeAChoice.Clear();
        roundResultWins.Clear();
        totalGameTime = 0;
        base.ResetCurrentAnalytics();
    }
}
