using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gratification_TurboRocket_StarsController : MonoBehaviour
{
    public void OnCaptured()
    {
        Gratification_TurboRocket_PlayerController.Instance.starsGatheredCount++;
        Deactivate();
        Debug.Log("caught star");
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
