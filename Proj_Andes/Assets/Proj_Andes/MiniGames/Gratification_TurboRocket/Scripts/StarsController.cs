using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarsController : MonoBehaviour
{
    public void OnCaptured()
    {
        PlayerController.Instance.starsGatheredCount++;
        Deactivate();
        Debug.Log("caught star");
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
