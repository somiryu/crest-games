using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gratification_TurboRocket_StarsController : MonoBehaviour
{
    [SerializeField] ParticleSystem capturedVFX;
    [SerializeField] AudioSource capturedSFX;
    public bool isCaptured;
    public void OnCaptured()
    {
        if (isCaptured) return;
         isCaptured = true;
        GetComponentInChildren<SpriteRenderer>().enabled = false;
        Gratification_TurboRocket_PlayerController.Instance.starsGatheredCount++;
        StartCoroutine( _OnCapturedwithDelay());
       
        Debug.Log("caught star");
    }
    IEnumerator _OnCapturedwithDelay()
    {
        capturedVFX.Play();
        capturedSFX.Play();
        yield return new WaitForSeconds(1f);
        Deactivate();
    }  
     
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
