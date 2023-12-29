using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

public class MonsterMarketManager : MonoBehaviour
{
    [SerializeField] Transform chestsContainer;
    [SerializeField] Transform chestOpenedContainer;
    [SerializeField] Button regularChest;
    [SerializeField] Button rareChest;
    [SerializeField] Button legendaryChest;

    [SerializeField] Button saveForLaterButton;
    [SerializeField] Button getChestButton;
    [SerializeField] Button collectBtn;
    [SerializeField] List<Monsters> regularMonsters = new List<Monsters>();
    [SerializeField] List<Monsters> rareMonsters = new List<Monsters>();
    [SerializeField] List<Monsters> legendaryMonsters = new List<Monsters>();

    List<Monsters> totalCollection = new List<Monsters>();
    [SerializeField] List<Monsters> currentMonstersFound = new List<Monsters>();
    private void Awake()
    {
        Init();
    }
    public void Init()
    {
        ResetList();
        chestsContainer.gameObject.SetActive(false);
        chestOpenedContainer.gameObject.SetActive(false);

        getChestButton.onClick.AddListener(GetChest);
        saveForLaterButton.onClick.AddListener(SaveForLater);
        collectBtn.onClick.AddListener(Collect);

        regularChest.onClick.AddListener(() => OpenChest(8, 2, 0, 2));
        rareChest.onClick.AddListener(() => OpenChest(2, 7, 1, 5));
        legendaryChest.onClick.AddListener(() => OpenChest(1, 5, 4, 8));
    }
    private void ResetList()
    {
        currentMonstersFound.Clear();
        for (int i = 0; i < regularMonsters.Count; i++) regularMonsters[i].monster.gameObject.SetActive(false);
        for (int i = 0; i < rareMonsters.Count; i++) rareMonsters[i].monster.gameObject.SetActive(false);
        for (int i = 0; i < legendaryMonsters.Count; i++) legendaryMonsters[i].monster.gameObject.SetActive(false);
    }
    void OpenChest(int regularMonstersLikelyness, int rareMonstersLikelyness, int legendaryMonstersLikelyness, int totalMonstersPerChest)
    {
        chestOpenedContainer.gameObject.SetActive(true);
        saveForLaterButton.gameObject.SetActive(true);
        getChestButton.gameObject.SetActive(false);

        for (int i = 0; i < totalMonstersPerChest; i++)
        {
            var newProb = GetRandomItem(10);
            Debug.Log(newProb);
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
        for (int i = 0; i < currentMonstersFound.Count; i++)
        {
            currentMonstersFound[i].monster.gameObject.SetActive(true);
            Debug.Log(currentMonstersFound[i].monsterType);
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
    }
    void SaveForLater()
    {
        chestsContainer.gameObject.SetActive(false);
        getChestButton.gameObject.SetActive(true);
        chestOpenedContainer.gameObject.SetActive(false);
    }
}

public enum MonsterType
{
    Regular,
    Rare,
    Legendary
}
[Serializable]
public struct Monsters
{
    public MonsterType monsterType;
    public SkinnableImage monster;
}