using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClosingController : MonoBehaviour
{
    [SerializeField] SimpleGameSequenceItem closingItem;
    [SerializeField] Transform logoPanel;
    [SerializeField] AudioClip finalAudio;
    [SerializeField] Button continueBtn;
    [SerializeField] GameObject rocketObj;

    AudioSource finalAudioSource;

    void Start()
    {
        logoPanel.gameObject.SetActive(false);
        TryGetComponent(out finalAudioSource);
        finalAudioSource.clip = finalAudio;
        finalAudioSource.Play();
        continueBtn.onClick.AddListener(closingItem.OnSequenceOver);
        TimeManager.Instance.gameState = SessionStateLeft.Finished;
        rocketObj.SetActive(GameSequencesList.isTheNarrativeSequence == false);

    }
}
