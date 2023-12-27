using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialoguesResponsesDisplayerUI : MonoBehaviour
{
    public Pool<ResponseBtn> responsesPool;
    private DialoguesDisplayerUI mainUi;

    [SerializeField] Button confirmationButton;


    public List<ResponseBtn> currResponses;
    public IDialoguesResponseDisplayerUser[] users;

    public ResponseBtn currHighlightedResponse;

    public void Init(DialoguesDisplayerUI _mainUI)
    {
        mainUi = _mainUI;
        responsesPool.Init(4);
        users = GetComponentsInChildren<IDialoguesResponseDisplayerUser>();
		for (int i = 0; i < users.Length; i++) users[i].Init(this);
        confirmationButton.onClick.AddListener(mainUi.OnClickResponseConfirmation);
        confirmationButton.gameObject.SetActive(false);
	}

	public void ShowResponses(DialogueResponse[] responseDatas)
    {
        for (int i = 0; i < users.Length; i++) users[i].OnShowResponses(responseDatas);

        for (int i = 0; i < responseDatas.Length; i++)
        {
            var newResponse = responsesPool.GetNewItem();
            newResponse.SetData(responseDatas[i]);
            newResponse.onClicked = HighlightResponse;
            currResponses.Add(newResponse);
			for (int j = 0; j < users.Length; j++) users[j].OnShowResponseBtn(newResponse);
		}
		gameObject.SetActive(true);
    }

    public void ActiveConfirmationButton(bool value)
    {
        confirmationButton.gameObject.SetActive(value);        
    }

    public void GrayOutResponse(int responseIdx)
    {
        currResponses[responseIdx].Btn.interactable = false;
    }

    public bool HasAvailableResponse()
    {
        for (int i = 0; i< currResponses.Count; i++)
        {
            if (currResponses[i].Btn.interactable) return true;
        }
        return false;
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
        if (currHighlightedResponse != null)
        {
            currHighlightedResponse.transform.localScale = Vector3.one; 
            currHighlightedResponse = null;
        }
		responsesPool.RecycleAll();
        gameObject.SetActive(false);
        currResponses.Clear();
    }
}

public interface IDialoguesResponseDisplayerUser
{
    public void Init(DialoguesResponsesDisplayerUI _mainUi);

    public void OnShowResponses(DialogueResponse[] responseDatas);

    public void OnShowResponseBtn(ResponseBtn responseBtn);

}
