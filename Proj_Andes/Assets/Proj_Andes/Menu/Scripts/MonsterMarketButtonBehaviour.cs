using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class MonsterMarketButtonBehaviour : MonoBehaviour, iMonsterMarketButton
{
    public MonsterMarketButton monsterMarketButton;
    [SerializeField] Button button;
    [SerializeField] Image lockedImage;
    [SerializeField] GameObject panelDisable;
    [SerializeField] Sprite spriteAbleBtn;
    [SerializeField] Sprite spriteDesableBtn;

    private void Start()
    {
        MonsterMarketManager.Instance.AddUserInterfaceMonsterButton(this);
        panelDisable.gameObject.SetActive(false);
        if (monsterMarketButton.monsterChestType == MonsterChestType.NONE) return;
        SetLockedImage();
    }
    public void SetLockedImage()
    {
        if (monsterMarketButton.monsterChestType == MonsterChestType.NONE) return;
        bool value = UserDataManager.CurrUser.Coins >= monsterMarketButton.costChest;
        lockedImage.gameObject.SetActive(!value);
    }

    public void SetInactiveState()
    {              
        button.image.sprite = spriteDesableBtn;
        panelDisable.gameObject.SetActive(true);
        if (monsterMarketButton.monsterChestType == MonsterChestType.NONE) return;       

    }

    public void SetActiveState()
    {
        button.image.sprite = spriteAbleBtn;
        panelDisable.gameObject.SetActive(false);
        if (monsterMarketButton.monsterChestType == MonsterChestType.NONE) return;
        SetLockedImage();

    }

}

[Serializable]
public class MonsterMarketButton
{
    public MonsterChestType monsterChestType;
    public Sprite chestOpenSprite;
    public Sprite chestCloseSprite;
    public int costChest;
}