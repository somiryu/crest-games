using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class MoveButtonSizeRockets : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    ISizeRocketsManager manager => ISizeRocketsManager.Instance;
    [SerializeField] Image btnImage;
    void Start()
    {
        TryGetComponent(out btnImage);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        btnImage.sprite = manager.gameConfigs.btnPressed;
        manager.shipIsMoving = true;
        if(manager.currShip != null) manager.currShip.shouldMove = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (manager.currShip != null) manager.currShip.shouldMove = false;
        manager.shipIsMoving = false;
        btnImage.sprite = manager.gameConfigs.btnUnPressed;
    }
}
