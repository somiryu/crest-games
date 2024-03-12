using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CatchCoinsAudioInstruction : MonoBehaviour, ITimeManagement
{
    static CatchCoinsAudioInstruction instance;
    public static CatchCoinsAudioInstruction Instance => instance;

    public AudioClip firstInstruction;
    [SerializeField] GameObject allScreenRayBlocker;

    AudioSource audioSource;
    public bool doneAudio;
    public bool startedCorr;
    AudioSource[] allAudioSources;
    bool[] allAudioSourcesInitialActiveState;
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
        allAudioSourcesInitialActiveState = new bool[allAudioSources.Length];
    }
    private void Start()
    {

        for (int i = 0; i < allAudioSourcesInitialActiveState.Length; i++)
        {
            allAudioSourcesInitialActiveState[i] = allAudioSources[i].isPlaying;
        }
		for (int i = 0; i < allAudioSources.Length; i++) allAudioSources[i].Pause();
        firstAudio = FirstInstruction();
        if(!GameUIController.Instance.onTuto) StartCoroutine(firstAudio);
    }

    float lastAudioPosition;

    public void StopAudioIns()
    {
        lastAudioPosition = audioSource.time;
        StopCoroutine(firstAudio);
		TimeManager.Instance.RemoveNewStopTimeUser(this);
		audioSource.Stop();
		allScreenRayBlocker.gameObject.SetActive(false);
	}

	public void RestartAudio()
    {
        if(firstAudio != null) StopCoroutine(firstAudio);
        firstAudio = FirstInstruction();
        StartCoroutine(firstAudio);
	}
    public IEnumerator FirstInstruction()
    {
        allScreenRayBlocker.gameObject.SetActive(true);
        startedCorr = true;
        audioSource.time = lastAudioPosition;
        audioSource.Play();
        TimeManager.Instance.SetNewStopTimeUser(this);
        yield return new WaitForSecondsRealtime(audioSource.clip.length - lastAudioPosition);
        TimeManager.Instance.RemoveNewStopTimeUser(this);
        doneAudio = true;
        for (int i = 0; i < allAudioSources.Length; i++)
        {
            if (!allAudioSourcesInitialActiveState[i]) continue;
            allAudioSources[i].Play();
        }
        audioSource.Stop();
        startedCorr = false;
		allScreenRayBlocker.gameObject.SetActive(false);
        firstAudio = null;
	}
}
