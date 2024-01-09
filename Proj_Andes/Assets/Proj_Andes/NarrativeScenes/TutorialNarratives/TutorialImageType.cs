using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialImageType : MonoBehaviour, iTutorialType
{
    [SerializeField] Image tutorialImage;
    public void StepStart(bool stepCompleted)
    {
        tutorialImage.gameObject.SetActive(true);
    }
    public void StepDone()
    {
        tutorialImage.gameObject.SetActive(false);
    }

}
