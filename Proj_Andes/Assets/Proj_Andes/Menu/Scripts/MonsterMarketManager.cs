using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonsterMarketManager : MonoBehaviour
{
    [SerializeField] Transform chestsContainer;
    [SerializeField] Transform chestOpenedContainer;
    [SerializeField] Button regularChest;
    [SerializeField] Button rareChest;
    [SerializeField] Button legendaryChest;

    [SerializeField] Button saveForLaterButton;
    [SerializeField] List<Image> regularMonsters = new List<Image>();
    [SerializeField] List<Image> rareMonsters = new List<Image>();
    [SerializeField] List<Image> legendaryMonsters = new List<Image>();

    List<Image> totalCollection = new List<Image>();
    List<Image> currentMonstersFound = new List<Image>();
    private void Awake()
    {
        Init();
    }
    public void Init()
    {
        chestsContainer.gameObject.SetActive(false);
        chestOpenedContainer.gameObject.SetActive(false);

        regularChest.onClick.AddListener(() => OpenChest(1, 1, 0));
        rareChest.onClick.AddListener(() => OpenChest(1, 3, 1));
        legendaryChest.onClick.AddListener(() => OpenChest(1, 4, 3));
    }

    void OpenChest(int regularMonstersAmt, int rareMonstersAmt, int legendaryMonstersAmt)
    {
        for (int i = 0; i < regularMonstersAmt; i++) currentMonstersFound.Add(regularMonsters[GetRandomItem(regularMonsters)]);
        for (int i = 0; i < rareMonstersAmt; i++) currentMonstersFound.Add(rareMonsters[GetRandomItem(rareMonsters)]);
        for (int i = 0; i < legendaryMonstersAmt; i++) currentMonstersFound.Add(legendaryMonsters[GetRandomItem(legendaryMonsters)]);

        for (int i = 0; i < currentMonstersFound.Count; i++)
        {
            currentMonstersFound[i].gameObject.SetActive(true);
        }
    }
    
    int GetRandomItem(List<Image> monsterList)
    {
        return Random.Range(0, monsterList.Count);
    }

    float CalculateLikelyness(int totalMonsters, float probability)
    {
        return (100 * probability) / totalMonsters;
    }
    void Update()
    {
        
    }
}

public enum ChestType
{
    Regular,
    Rare,
    Legendary
}