using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TryAgainManager : MonoBehaviour
{
    [SerializeField] SimpleGameSequenceItem tryAgainSeqItem;
    [SerializeField] Button retryBtn;
    [SerializeField] float waitFor;
    WaitForSeconds waitForSec => new WaitForSeconds(waitFor);
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GoToNextScene());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator GoToNextScene()
    {
        yield return waitForSec;
        tryAgainSeqItem.OnSequenceOver();
    }

}
