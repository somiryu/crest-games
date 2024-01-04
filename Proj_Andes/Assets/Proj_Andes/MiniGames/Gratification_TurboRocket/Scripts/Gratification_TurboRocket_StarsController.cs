using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Gratification_TurboRocket_StarsController : MonoBehaviour
{
    [SerializeField] ParticleSystem capturedVFX;
    [SerializeField] AudioSource capturedSFX;

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float step = 1f;
    [SerializeField] float yOfsetLimit = 0.1f;

    Vector3 initialPosition;
    bool isInitialPosition;
    public bool isCaptured;

    Gratification_TurboRocket_PlayerController player => Gratification_TurboRocket_PlayerController.Instance;

    private void Start()
    {
        initialPosition = transform.position;
        isInitialPosition = true;

    }
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
    
    public void OnTurbo()
    {
        if (isCaptured) return;
        

    }

    private void Update()
    {
        if (player.onTurbo)
        {
            MoveObjectToNearestEdge();
            isInitialPosition = false;
        }
        else if (!isInitialPosition)
        {
            ReturnObjectToInitialPosition();
        }
    }
    private void MoveObjectToNearestEdge()
    {
        float currDistance = player.CurrPos.y - transform.position.y;
        float newY = transform.position.y + (currDistance < 0 ? step : -step);

        float yViewport = Camera.main.WorldToViewportPoint(transform.position).y;

        if (yViewport > 0 + yOfsetLimit && yViewport < 1 - yOfsetLimit) 
        {
            var newPos = new Vector3(transform.position.x, newY, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, newPos, moveSpeed * Time.deltaTime);
        }
    }

    private void ReturnObjectToInitialPosition()
    {
        transform.position = Vector3.MoveTowards(transform.position, initialPosition, moveSpeed * Time.deltaTime);
        if(transform.position == initialPosition) isInitialPosition = true;
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
