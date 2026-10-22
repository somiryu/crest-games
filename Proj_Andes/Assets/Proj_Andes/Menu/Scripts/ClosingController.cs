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
        TimeManager.Instance.gameState = SessionStateLeft.Finalizado;
        //Reaching this scene is what makes the session finished: from here its data is queued for upload
        if (!GameSequencesList.isTheNarrativeSequence) UserDataManager.CommitFinishedSession();
        Debug.Log("<<<<<>>>>>>>>>> Start closing >> state " + TimeManager.Instance.gameState);
        rocketObj.SetActive(GameSequencesList.isTheNarrativeSequence == false);

    }
}
