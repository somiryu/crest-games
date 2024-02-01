using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TryAgainManager : MonoBehaviour
{
    public static int clickCounts;
    [SerializeField] Button retryBtn;
    [SerializeField] float waitFor;
    [SerializeField] Slider fakeLoadingSlider;

    IEnumerator changeSceneRoutineRef;
    void Start()
    {
        clickCounts = 0;
        fakeLoadingSlider.gameObject.SetActive(true);
		retryBtn.onClick.AddListener(() => clickCounts++);
        StartCoroutine(GoToNextScene());
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
        GameSequencesList.Instance.GoToNextItemInList();
    }

}
