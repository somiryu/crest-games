using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankTxt;
    [SerializeField] private Image image;
    private Monsters monster;

    public void Show(Monsters _monster)
    {
        monster = _monster;
        image.sprite = monster.sprite;
        rankTxt.SetText(monster.Name);
		gameObject.SetActive(true);
    }

    public string GetCurrMonsterID() => monster.guid;



}
