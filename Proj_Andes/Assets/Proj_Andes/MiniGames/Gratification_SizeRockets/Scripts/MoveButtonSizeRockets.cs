using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

[Serializable]
public class MoveButtonSizeRockets : Image, IPointerDownHandler, IPointerUpHandler
{
    ISizeRocketsManager manager => ISizeRocketsManager.Instance;
    public void OnPointerDown(PointerEventData eventData)
    {
        sprite = manager.gameConfigs.btnPressed;
        manager.shipIsMoving = true;
        if(manager.currShip != null) manager.currShip.shouldMove = true;

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (manager.currShip != null) manager.currShip.shouldMove = false;
        manager.shipIsMoving = false;
        sprite = manager.gameConfigs.btnUnPressed;
    }
}
