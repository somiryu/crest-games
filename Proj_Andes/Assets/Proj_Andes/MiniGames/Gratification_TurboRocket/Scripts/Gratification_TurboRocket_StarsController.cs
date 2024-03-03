using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;



public class Gratification_TurboRocket_StarsController : MonoBehaviour
{
    [SerializeField] ParticleSystem capturedVFX;
    [SerializeField] AudioSource capturedSFX;
    [SerializeField] SpriteRenderer graphic;

    [SerializeField] float moveSpeed = 5;
    [SerializeField] float step = 1f;
    [SerializeField] float yOfsetLimit = 0.1f;
    float targetSpeed;

    public Collider starColl;

    Vector3 initialPosition;
    public bool isInitialPosition;
    public bool isCaptured;

    iTurboRocketManager player => iTurboRocketManager.Instance;

    public void Init()
    {
        initialPosition = transform.position;
        isInitialPosition = true;
        TryGetComponent(out starColl);
    }
    public void OnCaptured()
    {
        if (isCaptured) return;
         isCaptured = true;
        GetComponentInChildren<SpriteRenderer>().enabled = false;
        iTurboRocketManager.Instance.starsGatheredCount++;
        StartCoroutine( _OnCapturedwithDelay());
       
        Debug.Log("caught star");
        player.OnScoreChanges?.Invoke();

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
    public Vector3 GetPos()
    {
        return transform.position;
    } 
    private void Update()
    {
        if (player.onTurbo)
        {
            MoveObjectToNearestEdge();
            isInitialPosition = false;
            var color = Color.white;
            color.a = 0.5f;
            graphic.color = color;
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
        targetSpeed = player.playerCurrentTargetSpeed - 2;
        var movX = Mathf.MoveTowards(transform.position.x, initialPosition.x, targetSpeed * Time.deltaTime);
        var movY = Mathf.MoveTowards(transform.position.y, initialPosition.y, targetSpeed * Time.deltaTime);
        transform.position = new Vector3(movX, movY);
        if(transform.position == initialPosition)
        {
            isInitialPosition = true;
            graphic.color = Color.white;
        }
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
