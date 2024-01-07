using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TryAgainManager : MonoBehaviour
{
    [SerializeField] SimpleGameSequenceItem tryAgainSeqItem;
    [SerializeField] Button retryBtn;
    [SerializeField] float waitFor;
    [SerializeField] Slider fakeLoadingSlider;

    IEnumerator changeSceneRoutineRef;

    void Start()
    {
		fakeLoadingSlider.gameObject.SetActive(false);
		retryBtn.onClick.AddListener(StartChangeSceneRoutine);
    }

    void StartChangeSceneRoutine()
    {
        if (changeSceneRoutineRef != null)
        {
            //TO DO: Store analytics of click counts.
            return;
        }
        fakeLoadingSlider.gameObject.SetActive(true);
        changeSceneRoutineRef = GoToNextScene();
        StartCoroutine(changeSceneRoutineRef);
    }

    IEnumerator GoToNextScene()
    {
        var timer = 0f;
        while (timer < waitFor)
        {
            timer += Time.deltaTime;
            var sliderValue = Mathf.Lerp(0, waitFor*2, timer);
            var progress = Mathf.InverseLerp(0, 10, sliderValue);
            fakeLoadingSlider.value = progress;
            yield return null;
        }
        tryAgainSeqItem.OnSequenceOver();
    }

}
