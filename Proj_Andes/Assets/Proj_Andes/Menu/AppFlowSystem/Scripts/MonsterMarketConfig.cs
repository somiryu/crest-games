using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterMarketConfig", menuName = "MonsterMarket/MonsterMarketConfig")]
public class MonsterMarketConfig : SimpleGameSequenceItem
{
    public int RegularChestPrice;
    public int RareChestPrice;
    public int LegendaryChestPrice;
    public MonstersLibrary monstersLibrary;


    public static bool isLastMarket;
    public static int marketAppearTimes = -1;
    public static int openChestTrials = 0;
    [NonSerialized] List<MonsterMarketRoundAnalytic> monsterMarketRoundAnalyticList = new List<MonsterMarketRoundAnalytic>();

	public int AvailableCoins => UserDataManager.CurrUser.Coins;
    public List<string> MyCollectionMonsters => UserDataManager.CurrUser.myCollectionMonsters;
	public override string GetSceneID() => DataIds.monsterMarket;

	public void ConsumeCoins(int amountToSubstract)
    {
        UserDataManager.CurrUser.Coins -= amountToSubstract;
	}

    public void AddMonsterToCollection(Monsters monster)
    {
        MyCollectionMonsters.Add(monster.guid);
    }

    public void SetAnalyticsInfo(List<MonsterMarketRoundAnalytic> roundAnalytics)
    {
		monsterMarketRoundAnalyticList = new List<MonsterMarketRoundAnalytic>(roundAnalytics);
	}

	public override void SaveAnalytics()
	{
		var currRoundAnalyticsDic = new Dictionary<string, object>();
		GameID = Guid.NewGuid().ToString();


		for (int i = 0; i < monsterMarketRoundAnalyticList.Count; i++)
        {
            var currInfo = monsterMarketRoundAnalyticList[i];
            currRoundAnalyticsDic.Clear();
            currRoundAnalyticsDic.Add(DataIds.GameID, GameID);
            currRoundAnalyticsDic.Add(DataIds.marketMonsterOrder, currInfo.marketIndex);
            currRoundAnalyticsDic.Add(DataIds.marketMonsterStarPre, currInfo.initialStars);
            currRoundAnalyticsDic.Add(DataIds.marketMonsterStarsSpent, currInfo.starsSpent);
            currRoundAnalyticsDic.Add(DataIds.marketMonsterStarsAfter, currInfo.finalStars);
            currRoundAnalyticsDic.Add(DataIds.marketMonsterChestTrial, currInfo.chestCount);
            currRoundAnalyticsDic.Add(DataIds.marketMonsterChestAnswer, currInfo.chestTypeOpenedString);
            currRoundAnalyticsDic.Add(DataIds.marketMonsterChestCode, currInfo.chestTypeOpenned);
            currRoundAnalyticsDic.Add(DataIds.marketMonsterTotalTime, currInfo.time);

			UserDataManager.SaveUserAnayticsPerGame(DataIds.monsterMarket, currRoundAnalyticsDic);
		}
	}
}
