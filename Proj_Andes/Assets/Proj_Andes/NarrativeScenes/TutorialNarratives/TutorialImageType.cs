using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialImageType : MonoBehaviour, iTutorialType
{
    [SerializeField] Image tutorialImage;
    [SerializeField] SpriteRenderer tutorialSprite;
    public void StepStart(bool ShouldActiveTut)
    {
        if(tutorialImage) tutorialImage.gameObject.SetActive(ShouldActiveTut);
        if(tutorialSprite) tutorialSprite.gameObject.SetActive(ShouldActiveTut);

	}
    public void StepDone()
    {
		if (tutorialImage) tutorialImage.gameObject.SetActive(false);
		if (tutorialSprite) tutorialSprite.gameObject.SetActive(false);
    }

}
