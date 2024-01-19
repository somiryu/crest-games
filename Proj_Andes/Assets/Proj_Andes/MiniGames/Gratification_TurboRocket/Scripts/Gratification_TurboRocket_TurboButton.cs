using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Gratification_TurboRocket_TurboButton : Image, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        iTurboRocketManager.Instance.OnEnterTurboMode();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        iTurboRocketManager.Instance.OnExitTurboMode();

    }
}
