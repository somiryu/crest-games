using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MonsterMarketButtonBehaviour : MonoBehaviour, iMonsterMarketButton
{
    [SerializeField] MonsterChestType monsterChestType;
    [SerializeField] Button button;
    [SerializeField] Image lockedImage;
    [SerializeField] Image chestImage;
    [SerializeField] Sprite spriteAbleBtn;
    [SerializeField] Sprite spriteDesableBtn;
    [SerializeField] int costChest;

    private void Start()
    {
        MonsterMarketManager.Instance.AddUserInterfaceMonsterButton(this);
        SetLockedImage();
    }
    public void SetLockedImage()
    {
        bool value = UserDataManager.CurrUser.Coins >= costChest;
        lockedImage.gameObject.SetActive(!value);
    }

    public void SetInactiveState()
    {
        button.image.sprite = spriteDesableBtn;
        lockedImage.gameObject.SetActive(false);
        ChangeAlphaValue(0.5f);
        
    }

    void ChangeAlphaValue(float newAlpha)
    {
        Color currentColor = chestImage.color;
        currentColor.a = newAlpha;
        chestImage.color = currentColor;
    }
}
