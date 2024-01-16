using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TutorialTurboButton_TurboRocket : Image, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        TutorialManager_Gratification_TurboRocket.Instance.OnEnterTurboMode();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        TutorialManager_Gratification_TurboRocket.Instance.OnExitTurboMode();

    }

}
