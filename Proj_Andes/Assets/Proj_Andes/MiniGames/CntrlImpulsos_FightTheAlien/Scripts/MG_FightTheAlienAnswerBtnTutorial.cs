using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MG_FightTheAlienAnswerBtnTutorial : MonoBehaviour
{
    [SerializeField] Image hiHighlightImg;

    public Button button;
    public AlienAttackOption alienAttackOption;
    public void ShowHighlightImg(bool value)
    {
        hiHighlightImg.gameObject.SetActive(value);
    }

    public void SetAnswerImage(Sprite image)
    {
        button.image.sprite = image;
    }
}
