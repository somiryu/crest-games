using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialoguesResponsesDisplayerUI : MonoBehaviour
{
    public Pool<ResponseBtn> responsesPool;
    private DialoguesDisplayerUI mainUi;

    public IDialoguesResponseDisplayerUser[] users;

	public void Init(DialoguesDisplayerUI _mainUI)
    {
        mainUi = _mainUI;
        responsesPool.Init(4);
        users = GetComponentsInChildren<IDialoguesResponseDisplayerUser>();
		for (int i = 0; i < users.Length; i++) users[i].Init(this);
	}

	public void ShowResponses(DialogueResponse[] responseDatas)
    {
        for (int i = 0; i < users.Length; i++) users[i].OnShowResponses(responseDatas);

        for (int i = 0; i < responseDatas.Length; i++)
        {
            var newResponse = responsesPool.GetNewItem();
            newResponse.SetData(responseDatas[i]);
            newResponse.onClicked = mainUi.OnClickResponse;
			for (int j = 0; j < users.Length; j++) users[j].OnShowResponseBtn(newResponse);
		}
		gameObject.SetActive(true);
    }

    public void Hide()
    {
        responsesPool.RecycleAll();
        gameObject.SetActive(false);
    }
}

public interface IDialoguesResponseDisplayerUser
{
    public void Init(DialoguesResponsesDisplayerUI _mainUi);

    public void OnShowResponses(DialogueResponse[] responseDatas);

    public void OnShowResponseBtn(ResponseBtn responseBtn);

}
