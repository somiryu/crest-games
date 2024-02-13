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

    public override string GetSceneID() => DataIds.fightTheAlienGame;

    public override void SaveAnalytics()
    {
        var currAnalytics = MG_FightTheAlienManager.Instance.AllRoundsAnalytics;
        itemAnalytics = new Dictionary<string, object>();
        for (int i = 0; i < currAnalytics.Count; i++)
        {
            itemAnalytics.Clear();
            itemAnalytics.Add(DataIds.fightTheAlienChallengeType, currAnalytics[i].challengeOrder);
            itemAnalytics.Add(DataIds.fightTheAlienRoundResultWin, currAnalytics[i].wonRound);
            itemAnalytics.Add(DataIds.fightTheAlientimeToMakeAChoice, currAnalytics[i].timeToMakeAChoice);
            itemAnalytics.Add(DataIds.fightTheAlienClicks, currAnalytics[i].clicks);
            itemAnalytics.Add(DataIds.fightTheAlienRanOutOfTime, currAnalytics[i].ranOutOfTime);

            UserDataManager.SaveUserAnayticsPerGame(DataIds.fightTheAlienGame, itemAnalytics);
        }
    }
    public override void ResetCurrentAnalytics()
    {
        base.ResetCurrentAnalytics();
    }
}
