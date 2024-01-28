using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MG_FightTheAlienPopUP : MonoBehaviour
{
    [SerializeField] Image alienAttackImage;
    [SerializeField] Image answerSelectedImage;
    [SerializeField] Image colorImage;
    [SerializeField] Image shapeImage;
    [SerializeField] Image faceFeedbackImage;
    [SerializeField] Image colorFeedbackImage;
    [SerializeField] Image shapeFeedbackImage;
    [SerializeField] Button closeButton;
    [SerializeField] Sprite shapeHeart;
    [SerializeField] Sprite shapeCirlce;
    [SerializeField] Sprite shapeStar;
    [SerializeField] Sprite happyFace;
    [SerializeField] Sprite sadFace;
    [SerializeField] Sprite correctFeedback;
    [SerializeField] Sprite wrongFeedback;

    [SerializeField] Color greenColor;
    [SerializeField] Color redColor;
    [SerializeField] Color blueColor;



    private void Start()
    {
        closeButton.onClick.AddListener(ClosePopUP);
    }

    private void ClosePopUP()
    {
        gameObject.SetActive(false);
    }
    public void SetAlienAttackImage(Sprite sprite)
    {
        alienAttackImage.sprite = sprite;       
    }

    
    public void SetAnswerSelectedImage(Sprite sprite)
    {
       answerSelectedImage.sprite = sprite;        
    }

    public void SetColorImage(colorAlienAttackConfig color, bool value)
    {
        switch (color)
        {
            case colorAlienAttackConfig.green:
                colorImage.color = greenColor;
                break;
            case colorAlienAttackConfig.blue:
                colorImage.color = blueColor;
                break;
            case colorAlienAttackConfig.red:
                colorImage.color = redColor;
                break;
        }

        if (!value) colorFeedbackImage.sprite = correctFeedback;
        else colorFeedbackImage.sprite = wrongFeedback;
    }
    public void SetShapeImage(shapeAlienAttackConfig shape, bool value)
    {
		//Comented this as we don't have the right sprites to fill this, but maybe will add later again
		/*
        switch (shape)
        {
            case shapeAlienAttackConfig.Star:
                shapeImage.sprite = shapeStar;
                break;
            case shapeAlienAttackConfig.Heart:
                shapeImage.sprite = shapeHeart;
                break;
            case shapeAlienAttackConfig.Circle:
                shapeImage.sprite = shapeCirlce;
                break;
        }
        */
		if (!value) shapeFeedbackImage.sprite = correctFeedback;
        else shapeFeedbackImage.sprite = wrongFeedback;
        
    }

    public void SetFaceFeedbackImage(bool value)
    {
        if (value)
            faceFeedbackImage.sprite = happyFace;
        else
            faceFeedbackImage.sprite = sadFace;       
    }


}
