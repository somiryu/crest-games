using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TryAgainManager : MonoBehaviour
{
    public static int clickCounts;
    [SerializeField] Button retryBtn;
    [SerializeField] float waitFor;
    [SerializeField] float extraWaitAtTheEnd;
    [SerializeField] Slider fakeLoadingSlider;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip tryAgainAudio;
    [SerializeField] AudioClip tryAgainAudio2;
    bool alreadyPlayed = false;
    void Start()
    {
        clickCounts = 0;
        fakeLoadingSlider.gameObject.SetActive(true);
		retryBtn.onClick.AddListener(() => clickCounts++);
        StartCoroutine(GoToNextScene());
        TryGetComponent(out audioSource);
        var introAudio = alreadyPlayed ? tryAgainAudio2 : tryAgainAudio;
        audioSource.clip = introAudio;
        audioSource.Play();
        alreadyPlayed = true;
    }
    IEnumerator GoToNextScene()
    {
        var timer = 0f;
        while (timer < waitFor)
        {
            timer += Time.deltaTime;
            var progress = Mathf.Clamp01(timer / waitFor);
            fakeLoadingSlider.value = progress;
            yield return null;
        }
        timer = 0f;
        while(timer < extraWaitAtTheEnd)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        GameSequencesList.Instance.GoToNextItemInList();
    }

}
