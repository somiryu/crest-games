using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MonsterMarketButtonBehaviour : MonoBehaviour, iMonsterMarketButton
{
    [SerializeField] MonsterChestType monsterChestType;
    [SerializeField] Button button;
    [SerializeField] Image lockedImage;
    [SerializeField] GameObject panelDisable;
    [SerializeField] Sprite spriteAbleBtn;
    [SerializeField] Sprite spriteDesableBtn;
    [SerializeField] int costChest;

    private void Start()
    {
        MonsterMarketManager.Instance.AddUserInterfaceMonsterButton(this);
        panelDisable.gameObject.SetActive(false);
        if (monsterChestType == MonsterChestType.NONE) return;
        SetLockedImage();
    }
    public void SetLockedImage()
    {
        if (monsterChestType == MonsterChestType.NONE) return;
        bool value = UserDataManager.CurrUser.Coins >= costChest;
        lockedImage.gameObject.SetActive(!value);
    }

    public void SetInactiveState(MonsterChestType _monsterChestType)
    {
        if (monsterChestType == _monsterChestType) return;       
        button.image.sprite = spriteDesableBtn;
        panelDisable.gameObject.SetActive(true);
        if (monsterChestType == MonsterChestType.NONE) return;
        lockedImage.gameObject.SetActive(false);

    }
    
}
