using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialoguesResponsesDisplayerUI : MonoBehaviour
{
    public Pool<ResponseBtn> responsesPool;
    private DialoguesDisplayerUI mainUi;

    public List<ResponseBtn> currResponses;

    public ResponseBtn currHighlightedResponse;

    public void Init(DialoguesDisplayerUI _mainUI)
    {
        mainUi = _mainUI;
        responsesPool.Init(4);
    }

    public void ShowResponses(DialogueResponse[] responseDatas)
    {
        for (int i = 0; i < responseDatas.Length; i++)
        {
            var newResponse = responsesPool.GetNewItem();
            newResponse.SetData(responseDatas[i]);
            newResponse.onClicked = HighlightResponse;
            currResponses.Add(newResponse);
        }
        gameObject.SetActive(true);
    }


    public void HighlightResponse(DialogueResponse response)
    {
        if(currHighlightedResponse != null) currHighlightedResponse.transform.localScale = Vector3.one;

        ResponseBtn btn = currResponses.Find(x => x.ResponseData == response);
        btn.transform.localScale = Vector3.one * 1.2f;
        currHighlightedResponse = btn;
        mainUi.OnClickResponse(response);

	}

    public void Hide()
    {
        responsesPool.RecycleAll();
        gameObject.SetActive(false);
        currResponses.Clear();
    }
}
