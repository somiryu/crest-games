using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankTxt;
    [SerializeField] private Image image;
    public Monsters monster;
    public Transform bk;

    public void Show(Monsters _monster, bool isCard)
    {
        monster = _monster;
        bk.TryGetComponent(out Image card);
        card.sprite = monster.cardBk;
        image.sprite = monster.sprite;
        rankTxt.SetText(monster.Name);
        bk.gameObject.SetActive(isCard);
        gameObject.SetActive(true);
    }

    public string GetCurrMonsterID() => monster.guid;



}
