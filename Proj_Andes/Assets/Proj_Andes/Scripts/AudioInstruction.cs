using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioInstruction : MonoBehaviour
{
    [SerializeField] AudioClip firstInstruction;
    AudioSource audioSource;
    float timer;
    [SerializeField] float targetHold;
    private void Awake()
    {
        TryGetComponent(out audioSource);
        audioSource.clip = firstInstruction;
        timer = 0;
    }
    private void Start()
    {
        //StartCoroutine(FirstInstruction());
    }
    private void Update()
    {
        /*
        timer += Time.deltaTime;
        if (timer < targetHold) Time.timeScale = 0;
        else Time.timeScale = 1;
        Debug.Log(Time.timeScale);*/
    }
    IEnumerator FirstInstruction()
    {
        audioSource.Play();
        yield return null;
    }
}
