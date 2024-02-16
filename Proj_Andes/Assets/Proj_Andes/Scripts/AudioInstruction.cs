using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioInstruction : MonoBehaviour, ITimeManagement
{
    static AudioInstruction instance;
    public static AudioInstruction Instance => instance;
    [SerializeField] AudioClip firstInstruction;
    AudioSource audioSource;
    public bool doneAudio;
    public bool startedCorr;
    AudioSource[] allAudioSources;
    public IEnumerator firstAudio => FirstInstruction();
    private void Awake()
    {
        if(instance != null && instance != this) DestroyImmediate(this); 
        instance = this;
        TryGetComponent(out audioSource);
        audioSource.clip = firstInstruction;
        doneAudio = false;
        startedCorr = false;
        allAudioSources = FindObjectsOfType<AudioSource>();
    }
    private void Start()
    {
        for (int i = 0; i < allAudioSources.Length; i++) allAudioSources[i].Pause();
        StartCoroutine(FirstInstruction());
    }
    public void StopAudioIns()
    {
        StopCoroutine(FirstInstruction());
        audioSource.Stop();
    }
    public IEnumerator FirstInstruction()
    {
        startedCorr = true;
        audioSource.Play();
        TimeManager.Instance.SetNewStopTimeUser(this);
        if (GameUIController.Instance != null) while (GameUIController.Instance.onPause) yield return null;
        yield return new WaitForSecondsRealtime(audioSource.clip.length);
        TimeManager.Instance.RemoveNewStopTimeUser(this);
        doneAudio = true;
        for (int i = 0; i < allAudioSources.Length; i++) allAudioSources[i].Play();
        audioSource.Stop();
        startedCorr = false;
    }
}
