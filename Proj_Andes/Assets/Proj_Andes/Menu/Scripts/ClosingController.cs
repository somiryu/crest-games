using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClosingController : MonoBehaviour
{
    [SerializeField] SimpleGameSequenceItem closingItem;
    [SerializeField] float waitFor;
    [SerializeField] Transform logoPanel;
    [SerializeField] AudioClip finalAudio;
    AudioSource finalAudioSource;
    WaitForSeconds waitForSec;
    void Start()
    {
        waitForSec = new WaitForSeconds(waitFor);
        logoPanel.gameObject.SetActive(false);
        TryGetComponent(out finalAudioSource);
        finalAudioSource.clip = finalAudio;
        finalAudioSource.Play();
        StartCoroutine(GoToNextScene());
    }
    IEnumerator GoToNextScene()
    {
        yield return waitForSec;
        logoPanel.gameObject.SetActive(true);
        yield return waitForSec;
        closingItem.OnSequenceOver();

    }
}
