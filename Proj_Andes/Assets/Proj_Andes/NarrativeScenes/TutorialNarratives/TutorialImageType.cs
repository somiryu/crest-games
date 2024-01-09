using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialImageType : TutorialType
{
    [SerializeField] Image tutorialImage;
    public override void StepStart(bool stepCompleted)
    {
        tutorialImage.gameObject.SetActive(true);
    }
    public override void StepDone()
    {
        tutorialImage.gameObject.SetActive(false);
    }

}
