using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Gratification_TurboRocket_StarsController : MonoBehaviour
{
    [SerializeField] ParticleSystem capturedVFX;
    [SerializeField] AudioSource capturedSFX;

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float screenWidthLimit = 5f;
    Vector3 initialPosition;
    public bool isCaptured;

    private void Start()
    {
        initialPosition = transform.position;
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
        if (Input.GetKey(KeyCode.Space))
        {
            MoveObjectToNearestEdge();
        }
        else
        {
            ReturnObjectToInitialPosition();
        }
    }
    private void MoveObjectToNearestEdge()
    {
        float screenHeight = Camera.main.orthographicSize * 2f;
        float newY = Mathf.Clamp(transform.position.y + moveSpeed * Time.deltaTime, initialPosition.y - screenHeight / 2, initialPosition.y + screenHeight / 2);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void ReturnObjectToInitialPosition()
    {
        transform.position = Vector3.MoveTowards(transform.position, initialPosition, moveSpeed * Time.deltaTime);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}
