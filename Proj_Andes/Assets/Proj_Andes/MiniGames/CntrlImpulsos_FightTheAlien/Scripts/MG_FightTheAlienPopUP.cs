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
                colorImage.color = Color.green;
                break;
            case colorAlienAttackConfig.blue:
                colorImage.color = Color.blue;
                break;
            case colorAlienAttackConfig.red:
                colorImage.color = Color.red;
                break;
        }

        if (!value) colorFeedbackImage.sprite = correctFeedback;
        else colorFeedbackImage.sprite = wrongFeedback;
    }
    public void SetShapeImage(shapeAlienAttackConfig shape, bool value)
    {
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
