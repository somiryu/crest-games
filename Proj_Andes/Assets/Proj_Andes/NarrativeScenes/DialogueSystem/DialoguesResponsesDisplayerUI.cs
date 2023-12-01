using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialoguesResponsesDisplayerUI : MonoBehaviour
{
    public Pool<ResponseBtn> responsesPool;
    private DialoguesDisplayerUI mainUi;

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
            newResponse.onClicked = mainUi.OnClickResponse;
        }
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        responsesPool.RecycleAll();
        gameObject.SetActive(false);
    }
}
