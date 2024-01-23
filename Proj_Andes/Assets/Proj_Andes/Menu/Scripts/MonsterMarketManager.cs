using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using TMPro;
using JetBrains.Annotations;
using Unity.VisualScripting;
using Tymski;

public class MonsterMarketManager : MonoBehaviour
{
    static MonsterMarketManager instance;
    public static MonsterMarketManager Instance => instance;
    [SerializeField] MonsterMarketConfig marketConfig;

    [SerializeField] Transform chestsContainer;
    [SerializeField] Transform chestOpenedContainer;

    [SerializeField] Button regularChest;
    [SerializeField] Button rareChest;
    [SerializeField] Button legendaryChest;

    [SerializeField] Button saveForLaterButton;
    [SerializeField] Button getChestButton;
    [SerializeField] Button collectBtn;
    [SerializeField] TextMeshProUGUI coinsAmtTxt;

    [SerializeField] MyCollectionManager myCollectionManager;

    public Pool<MonsterItemUI> monstersUIInChestOpenning;
    List<Monsters> totalDataCollection => MyCollectionManager.totalDataCollection;
    List<Monsters> currentMonstersFound = new List<Monsters>();

    private void Awake()
    {
        if(instance != null)
        {
            if(instance != this)
            {
                DestroyImmediate(this);
            }
        }
        instance = this;
        Init();
    }
    public void Init()
    {
        myCollectionManager.Init();

        RefreshCollectionFromData();

        monstersUIInChestOpenning.Init(5);
        monstersUIInChestOpenning.RecycleAll();

        chestsContainer.gameObject.SetActive(false);
        chestOpenedContainer.gameObject.SetActive(false);

        getChestButton.onClick.AddListener(GetChest);
        saveForLaterButton.onClick.AddListener(SaveForLater);
        collectBtn.onClick.AddListener(Collect);

        regularChest.onClick.AddListener(() => BuyChest(MonsterChestType.Regular));
        rareChest.onClick.AddListener(() => BuyChest(MonsterChestType.Rare));
        legendaryChest.onClick.AddListener(() => BuyChest(MonsterChestType.Legendary));

        coinsAmtTxt.text = marketConfig.AvailableCoins.ToString();
    }

    void BuyChest(MonsterChestType type)
    {
        switch (type)
        {
            case MonsterChestType.Regular:
                if(marketConfig.AvailableCoins >= marketConfig.RegularChestPrice)
                {
                    marketConfig.ConsumeCoins(marketConfig.RegularChestPrice);
                    OpenChest(80, 20, 0, 2);
                }
                else Debug.Log("¡No tienes suficientes monedas!");
                break;
            case MonsterChestType.Rare:
                if (marketConfig.AvailableCoins >= marketConfig.RareChestPrice)
                {
                    marketConfig.ConsumeCoins(marketConfig.RareChestPrice);
                    OpenChest(20, 70, 10, 5);
                }
                else Debug.Log("¡No tienes suficientes monedas!");
                break;
            case MonsterChestType.Legendary:
                if (marketConfig.AvailableCoins >= marketConfig.LegendaryChestPrice)
                {
                    marketConfig.ConsumeCoins(marketConfig.LegendaryChestPrice);
                    OpenChest(5, 50, 45, 8);
                }
                else Debug.Log("¡No tienes suficientes monedas!");
                break;
        }
        coinsAmtTxt.text = marketConfig.AvailableCoins.ToString();
    }

    void OpenChest(int regularMonstersLikelyness, int rareMonstersLikelyness, int legendaryMonstersLikelyness, int totalMonstersPerChest)
    {
        chestOpenedContainer.gameObject.SetActive(true);
        saveForLaterButton.gameObject.SetActive(true);
        getChestButton.gameObject.SetActive(false);

        currentMonstersFound.Clear();

        var totalChance = regularMonstersLikelyness + rareMonstersLikelyness + legendaryMonstersLikelyness;

        for (int i = 0; i < totalMonstersPerChest; i++)
        {
            var newProb = GetRandomItem(totalChance+1);
            if(newProb <= regularMonstersLikelyness && regularMonstersLikelyness > 0)
            {
                currentMonstersFound.Add(marketConfig.monstersLibrary.GetRandomMonster(MonsterChestType.Regular));
            }
            else if(newProb > regularMonstersLikelyness && newProb <= regularMonstersLikelyness + rareMonstersLikelyness && rareMonstersLikelyness > 0)
            {
				currentMonstersFound.Add(marketConfig.monstersLibrary.GetRandomMonster(MonsterChestType.Rare));
            }
            else if(newProb > 0 && newProb > regularMonstersLikelyness + rareMonstersLikelyness && legendaryMonstersLikelyness > 0)
            {
				currentMonstersFound.Add(marketConfig.monstersLibrary.GetRandomMonster(MonsterChestType.Legendary));
            }
        }

        monstersUIInChestOpenning.RecycleAll();

        for (int i = 0; i < currentMonstersFound.Count; i++)
        {
            var newItem = monstersUIInChestOpenning.GetNewItem();
            newItem.Show(currentMonstersFound[i]);
        }
    }
    public void RefreshCollectionFromData()
    {
        totalDataCollection.Clear();
        for (int i = 0; i < marketConfig.MyCollectionMonsters.Count; i++)
        {
            var currID = marketConfig.MyCollectionMonsters[i];
            var monsterFound = marketConfig.monstersLibrary.GetMonsterByID(currID);
            if (monsterFound == null) continue;
            totalDataCollection.Add(monsterFound);
        }
        myCollectionManager.monstersUIInCollection.RecycleAll();
    }


    int GetRandomItem(int itemList)
    {
        return Random.Range(0, itemList);
    }

    void GetChest()
    {
        chestsContainer.gameObject.SetActive(true);
        getChestButton.gameObject.SetActive(false);
    }

    void Collect()
    {
        chestsContainer.gameObject.SetActive(false);
        chestOpenedContainer.gameObject.SetActive(false);
        getChestButton.gameObject.SetActive(true);

        for (int i = 0; i < currentMonstersFound.Count; i++)
        {
            if (!totalDataCollection.Contains(currentMonstersFound[i]))
            {
                marketConfig.AddMonsterToCollection(currentMonstersFound[i]);
                Debug.Log("Encontraste un nuevo monstruo "+ currentMonstersFound[i].Name);
            }
            else Debug.Log("Monstruo repetido!");
        }
        RefreshCollectionFromData();

	}



    void SaveForLater()
    {
        chestsContainer.gameObject.SetActive(false);
        getChestButton.gameObject.SetActive(true);
        chestOpenedContainer.gameObject.SetActive(false);
        marketConfig.OnSequenceOver();
    }
}

public enum MonsterChestType
{
    Regular,
    Rare,
    Legendary
}
[Serializable]
public class Monsters
{
    public string Name;
    public string guid = new Guid().ToString();
    public MonsterChestType monsterType;
    public int monsterIndex;
    public Sprite sprite;

    public void OnValidate()
    {
        if (string.IsNullOrEmpty(guid))
        {
            guid = Guid.NewGuid().ToString();  
        }
        var currName = monsterType.ToString() + " : " + sprite.name;
		if (Name != currName)
        {
            Name = currName;
        }
    }
}