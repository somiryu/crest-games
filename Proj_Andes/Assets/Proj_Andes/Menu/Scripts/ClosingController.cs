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
    [SerializeField] Button continueBtn;

    AudioSource finalAudioSource;
    WaitForSeconds waitForSec;

    void Start()
    {
        waitForSec = new WaitForSeconds(waitFor);
        logoPanel.gameObject.SetActive(false);
        TryGetComponent(out finalAudioSource);
        finalAudioSource.clip = finalAudio;
        finalAudioSource.Play();
        continueBtn.onClick.AddListener(closingItem.OnSequenceOver);
        TimeManager.Instance.gameState = GameStateLeft.Finished;

    }

}
