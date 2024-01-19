using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialTurboButton_TurboRocket : Image, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        iTurboRocketManager.Instance.OnEnterTurboMode();
        color = Color.white;
        if (iTurboRocketManager.Instance is TutorialManager_Gratification_TurboRocket manager)
        {
            if (manager.currTutoStep.step == TutorialStepsTurboRocket.TurboAppear) manager.currTutoStep.EndTutoStep();
        }

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        iTurboRocketManager.Instance.OnExitTurboMode();
        if (iTurboRocketManager.Instance is TutorialManager_Gratification_TurboRocket manager)
        {
            if (manager.currTutoStep.step == TutorialStepsTurboRocket.TurboAppear || manager.currTutoStep.step == TutorialStepsTurboRocket.UnclickTurbo)
            {
                manager.currTutoStep.EndTutoStep();
                color = Color.white;
                manager.EndOfTuto();
            }
        }
    }

}
