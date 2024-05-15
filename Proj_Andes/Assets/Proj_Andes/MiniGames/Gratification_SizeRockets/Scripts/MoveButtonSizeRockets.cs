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


    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pressed to move ship");
        btnImage.sprite = manager.gameConfigs.btnPressed;
        manager.shipIsMoving = true;
        if (manager.currShip != null)
        {
            manager.currShip.shouldMove = true;
			Debug.Log("ship found");
		}
		else
        {
            Debug.Log("No ship found");
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
		Debug.Log("released to move ship");
        if (manager.currShip != null)
        {
            manager.currShip.shouldMove = false;
			Debug.Log("ship found");
		}
		else
        {
			Debug.Log("No ship found");
		}
		manager.shipIsMoving = false;
        btnImage.sprite = manager.gameConfigs.btnUnPressed;
    }
}
