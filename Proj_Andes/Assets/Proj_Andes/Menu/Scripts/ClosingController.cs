using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClosingController : MonoBehaviour
{
    [SerializeField] SimpleGameSequenceItem closingItem;
    [SerializeField] float waitFor;
    [SerializeField] Transform logoPanel;
    WaitForSeconds waitForSec;
    void Start()
    {
        waitForSec = new WaitForSeconds(waitFor);
        logoPanel.gameObject.SetActive(false);
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
