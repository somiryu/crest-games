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

public interface iMonsterMarketButton
{    
    public void SetLockedImage();
}
public class MonsterMarketManager : MonoBehaviour
{
    static MonsterMarketManager instance;

    [SerializeField] GameObject UI_BG_base;

    public static MonsterMarketManager Instance => instance;
    [SerializeField] MonsterMarketConfig marketConfig;

    [SerializeField] Transform chestOpenedContainer;
    [SerializeField] Image chestOpenedContainerImg;
    [SerializeField] GameObject chestNoEnoughCoins;

    [SerializeField] Button regularChest;
    [SerializeField] Button rareChest;
    [SerializeField] Button legendaryChest;

    [SerializeField] List<MonsterMarketButtonBehaviour> userMonsterButtonBehaviours;

    [SerializeField] Button saveForLaterButton;
    [SerializeField] Button confirmButton;
    [SerializeField] Button collectBtn;
    [SerializeField] TextMeshProUGUI coinsAmtTxt;
    [SerializeField] Button chestOpenButton;
    [SerializeField] GameObject chestOpenButtonParent;
    [SerializeField] Image chestOpenImg;

    [SerializeField] MyCollectionManager myCollectionManager;

    public Pool<MonsterItemUI> monstersUIInChestOpenning;
    List<Monsters> totalDataCollection => MyCollectionManager.totalDataCollection;
    List<Monsters> currentMonstersFound = new List<Monsters>();
    MonsterChestType currMonsterChestType;
    MonsterMarketButtonBehaviour currButton;

    Button currSelectedButton;

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

    public void AddUserInterfaceMonsterButton(MonsterMarketButtonBehaviour monsterMarketButtonBehaviour)
    {
        userMonsterButtonBehaviours.Add(monsterMarketButtonBehaviour);
    }
    public void RemoveUserInterfaceMonsterButton(MonsterMarketButtonBehaviour monsterMarketButtonBehaviour)
    {
        userMonsterButtonBehaviours.Remove(monsterMarketButtonBehaviour);
    }



    public void Init()
    {        
        myCollectionManager.Init(false);
        myCollectionManager.HideCollection();
        confirmButton.gameObject.SetActive(false);
        chestOpenButtonParent.gameObject.SetActive(false);
        RefreshCollectionFromData();

        monstersUIInChestOpenning.Init(5);
        monstersUIInChestOpenning.RecycleAll();

        chestOpenedContainer.gameObject.SetActive(false);

        confirmButton.onClick.AddListener(OpenButtonBeforeBuyChestOrContinuing);
        chestOpenButton.onClick.AddListener(BuyChest);
        collectBtn.onClick.AddListener(Collect);

        regularChest.onClick.AddListener(() => ActiveConfirmationButton(MonsterChestType.Regular, regularChest));
        rareChest.onClick.AddListener(() => ActiveConfirmationButton(MonsterChestType.Rare, rareChest));
        legendaryChest.onClick.AddListener(() => ActiveConfirmationButton(MonsterChestType.Legendary, legendaryChest));
        saveForLaterButton.onClick.AddListener(() => ActiveConfirmationButton(MonsterChestType.NONE, saveForLaterButton));

        coinsAmtTxt.text = marketConfig.AvailableCoins.ToString();
    }

    void ActiveConfirmationButton(MonsterChestType monsterChestType, Button chestBtn) 
    {
        currSelectedButton = chestBtn;
        chestBtn.TryGetComponent<MonsterMarketButtonBehaviour>(out currButton);    
        
        currMonsterChestType = monsterChestType;

        for (int i = 0; i < userMonsterButtonBehaviours.Count; i++)
        {
            if (currButton.monsterMarketButton.costChest > marketConfig.AvailableCoins)
            {
                userMonsterButtonBehaviours[i].SetActiveState();
                confirmButton.gameObject.SetActive(false);
                BuyChest();
            }
            else
            {
                if (userMonsterButtonBehaviours[i].monsterMarketButton.monsterChestType == monsterChestType) userMonsterButtonBehaviours[i].SetActiveState();
                else userMonsterButtonBehaviours[i].SetInactiveState();
                confirmButton.gameObject.SetActive(true);
            }
        }

    }

    void OpenButtonBeforeBuyChestOrContinuing()
    {
        if(currSelectedButton == saveForLaterButton) SaveForLater();
        else
        {
            chestOpenButtonParent.gameObject.SetActive(true);
            chestOpenImg.sprite = currButton.monsterMarketButton.chestCloseSprite;
            confirmButton.gameObject.SetActive(false);
            UI_BG_base.SetActive(false);
        }
    }

    void BuyChest()
    {
        chestOpenButtonParent.gameObject.SetActive(false);

        switch (currMonsterChestType)
        {
            case MonsterChestType.Regular:
                if (marketConfig.AvailableCoins >= marketConfig.RegularChestPrice)
                {
                    marketConfig.ConsumeCoins(marketConfig.RegularChestPrice);
                    OpenChest(100, 0, 0, 1);
                }
                else chestNoEnoughCoins.SetActive(true);
                break;
            case MonsterChestType.Rare:
                if (marketConfig.AvailableCoins >= marketConfig.RareChestPrice)
                {
                    marketConfig.ConsumeCoins(marketConfig.RareChestPrice);
                    OpenChest(50, 50, 0, 2);
                }
                else chestNoEnoughCoins.SetActive(true);
                break;
            case MonsterChestType.Legendary:
                if (marketConfig.AvailableCoins >= marketConfig.LegendaryChestPrice)
                {
                    marketConfig.ConsumeCoins(marketConfig.LegendaryChestPrice);
                    OpenChest(33, 33, 33, 3);
                }
                else chestNoEnoughCoins.SetActive(true);
                break;
        }
        coinsAmtTxt.text = marketConfig.AvailableCoins.ToString();
        SetLockedImageInButtons();

    }

    private void SetLockedImageInButtons()
    {
        for (int i = 0; i < userMonsterButtonBehaviours.Count; i++)
        {
            userMonsterButtonBehaviours[i].SetLockedImage();
        }

    }
    void OpenChest(int regularMonstersLikelyness, int rareMonstersLikelyness, int legendaryMonstersLikelyness, int totalMonstersPerChest)
    {
        chestOpenButtonParent.gameObject.SetActive(false);
        chestOpenedContainer.gameObject.SetActive(true);
        saveForLaterButton.gameObject.SetActive(true);
        chestOpenedContainerImg.sprite = currButton.monsterMarketButton.chestOpenSprite;
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
    }


    int GetRandomItem(int itemList)
    {
        return Random.Range(0, itemList);
    }

    void Collect()
    {
        chestOpenedContainer.gameObject.SetActive(false);
        UI_BG_base.SetActive(true);

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
		myCollectionManager.ShowCollection();
        myCollectionManager.gameObject.SetActive(true);

    }


    public void SaveForLater()
    {
        chestOpenedContainer.gameObject.SetActive(false);
        marketConfig.OnSequenceOver();
    }
}

public enum MonsterChestType
{
    Regular,
    Rare,
    Legendary,
    NONE
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