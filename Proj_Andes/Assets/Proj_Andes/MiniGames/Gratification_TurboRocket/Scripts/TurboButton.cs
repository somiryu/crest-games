using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TurboButton : Image, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        PlayerController.Instance.OnEnterTurboMode();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PlayerController.Instance.OnExitTurboMode();

    }
}
