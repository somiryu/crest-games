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

    public void SetAnalyticsInfo(Dictionary<string, object> analytics)
    {
        itemAnalytics = new Dictionary<string, object>(analytics);
    }

	public override void SaveAnalytics()
	{
        GameID = Guid.NewGuid().ToString();
		var dictionary = new Dictionary<string, object>();
		dictionary.Add(DataIds.GameID, GameID);
        dictionary.AddRange(itemAnalytics);
		UserDataManager.SaveUserAnayticsPerGame(DataIds.monsterMarket, dictionary);
	}
}
