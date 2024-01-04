using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using TMPro;
using JetBrains.Annotations;

public class MonsterMarketManager : MonoBehaviour
{
    static MonsterMarketManager instance;
    public static MonsterMarketManager Instance => instance;
    [SerializeField] MonsterMarketConfig marketConfig;

    [SerializeField] Transform chestsContainer;
    [SerializeField] Transform chestOpenedContainer;
    [SerializeField] Transform monsterImagesContainer;
    Image[] monsterImages;
    [SerializeField] Button regularChest;
    [SerializeField] Button rareChest;
    [SerializeField] Button legendaryChest;

    [SerializeField] Button saveForLaterButton;
    [SerializeField] Button getChestButton;
    [SerializeField] Button collectBtn;
    [SerializeField] TextMeshProUGUI coinsAmtTxt;
    [SerializeField] List<Monsters> regularMonsters = new List<Monsters>();
    [SerializeField] List<Monsters> rareMonsters = new List<Monsters>();
    [SerializeField] List<Monsters> legendaryMonsters = new List<Monsters>();


    [SerializeField] List<Monsters> totalCollection = new List<Monsters>();
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
        monsterImages = monsterImagesContainer.GetComponentsInChildren<Image>();
        marketConfig.InitConfig();
        InitLists();
        chestsContainer.gameObject.SetActive(false);
        chestOpenedContainer.gameObject.SetActive(false);

        getChestButton.onClick.AddListener(GetChest);
        saveForLaterButton.onClick.AddListener(SaveForLater);
        collectBtn.onClick.AddListener(Collect);

        regularChest.onClick.AddListener(() => BuyChest(MonsterChestType.Regular));
        rareChest.onClick.AddListener(() => BuyChest(MonsterChestType.Rare));
        legendaryChest.onClick.AddListener(() => BuyChest(MonsterChestType.Legendary));

        coinsAmtTxt.text = marketConfig.availableCoins.ToString();
    }
    [ContextMenu("ResetCollection")]
    private void InitLists()
    {
        totalCollection = marketConfig.myCollectionMonsters;
        for (int i = 0; i < monsterImages.Length; i++) monsterImages[i].gameObject.SetActive(false);
        ResetLists(regularMonsters);
        ResetLists(rareMonsters);
        ResetLists(legendaryMonsters);
    }
    void ResetLists(List<Monsters> monsterList)
    {
        for (int i = 0; i < monsterList.Count; i++)
        {
            monsterList[i].monsterIndex = i;
            monsterList[i].monster.gameObject.SetActive(false);
        }
    }
    void BuyChest(MonsterChestType type)
    {
        switch (type)
        {
            case MonsterChestType.Regular:
                if(marketConfig.availableCoins >= marketConfig.RegularChestPrice)
                {
                    marketConfig.availableCoins -= marketConfig.RegularChestPrice;
                    OpenChest(80, 20, 0, 2);
                }
                else Debug.Log("¡No tienes suficientes monedas!");
                break;
            case MonsterChestType.Rare:
                if (marketConfig.availableCoins >= marketConfig.RareChestPrice)
                {
                    marketConfig.availableCoins -= marketConfig.RareChestPrice;
                    OpenChest(20, 70, 10, 5);
                }
                else Debug.Log("¡No tienes suficientes monedas!");
                break;
            case MonsterChestType.Legendary:
                if (marketConfig.availableCoins >= marketConfig.LegendaryChestPrice)
                {
                    marketConfig.availableCoins -= marketConfig.LegendaryChestPrice;
                    OpenChest(5, 50, 45, 8);
                }
                else Debug.Log("¡No tienes suficientes monedas!");
                break;
        }
        coinsAmtTxt.text = marketConfig.availableCoins.ToString();
    }
    void OpenChest(int regularMonstersLikelyness, int rareMonstersLikelyness, int legendaryMonstersLikelyness, int totalMonstersPerChest)
    {
        chestOpenedContainer.gameObject.SetActive(true);
        saveForLaterButton.gameObject.SetActive(true);
        getChestButton.gameObject.SetActive(false);

        currentMonstersFound.Clear();

        for (int i = 0; i < totalMonstersPerChest; i++)
        {
            var newProb = GetRandomItem(100);
            if(newProb <= regularMonstersLikelyness)
            {
                currentMonstersFound.Add(regularMonsters[GetRandomItem(regularMonsters.Count)]);
            }
            else if(newProb > regularMonstersLikelyness && newProb <= regularMonstersLikelyness + rareMonstersLikelyness)
            {
                currentMonstersFound.Add(rareMonsters[GetRandomItem(rareMonsters.Count)]);
            }
            else if(newProb > 0 && newProb > regularMonstersLikelyness + rareMonstersLikelyness && legendaryMonstersLikelyness > 0)
            {
                currentMonstersFound.Add(legendaryMonsters[GetRandomItem(legendaryMonsters.Count)]);
            }
        }
        for (int i = 0; i < monsterImages.Length; i++) monsterImages[i].gameObject.SetActive(false);
        for (int i = 0; i < currentMonstersFound.Count; i++)
        {
            monsterImages[i].sprite = currentMonstersFound[i].monster.sRenderer.sprite;
            monsterImages[i].gameObject.SetActive(true);
        }
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
            if (!totalCollection.Contains(currentMonstersFound[i]))
            {
                totalCollection.Add(currentMonstersFound[i]);
                currentMonstersFound[i].monster.gameObject.SetActive(true);
                Debug.Log("Encontraste un nuevo monstruo "+ currentMonstersFound[i].monsterType);
            }
            else
            {
                Debug.Log("Monstruo repetido!");
            }
        }
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
    public MonsterChestType monsterType;
    public int monsterIndex;
    public SkinnableImage monster;
}