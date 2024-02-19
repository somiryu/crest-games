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
    public IEnumerator firstAudio;
    private void Awake()
    {
        if(instance != null && instance != this) DestroyImmediate(this); 
        instance = this;
        TryGetComponent(out audioSource);
        audioSource.clip = firstInstruction;
        doneAudio = false;
        startedCorr = false;
        lastAudioPosition = 0;
        allAudioSources = FindObjectsOfType<AudioSource>();
    }
    private void Start()
    {
        for (int i = 0; i < allAudioSources.Length; i++) allAudioSources[i].Pause();
        firstAudio = FirstInstruction();
        StartCoroutine(firstAudio);
    }

    float lastAudioPosition;

    public void StopAudioIns()
    {
        lastAudioPosition = audioSource.time;
        StopCoroutine(firstAudio);
		TimeManager.Instance.RemoveNewStopTimeUser(this);
		audioSource.Stop();
    }

    public void RestartAudio()
    {
        firstAudio = FirstInstruction();
        StartCoroutine(firstAudio);
	}
    public IEnumerator FirstInstruction()
    {
        startedCorr = true;
        audioSource.time = lastAudioPosition;
        audioSource.Play();
        TimeManager.Instance.SetNewStopTimeUser(this);
        yield return new WaitForSecondsRealtime(audioSource.clip.length - lastAudioPosition);
        TimeManager.Instance.RemoveNewStopTimeUser(this);
        doneAudio = true;
        for (int i = 0; i < allAudioSources.Length; i++) allAudioSources[i].Play();
        audioSource.Stop();
        startedCorr = false;
    }
}
