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

	public bool audibleResponses = true;
	public List<ResponseBtn> currResponses;
	public IDialoguesResponseDisplayerUser[] users;

	public ResponseBtn currHighlightedResponse;

	public List<Sprite> highlightBtnsSprites;
	public Sprite disableBtnContainer;

	public bool UseConfirmationBtn = true;

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
			newResponse.onClicked = OnClickedResponse;
			currResponses.Add(newResponse);
			Sprite highlightSprite = null;
			if(i < highlightBtnsSprites.Count)
			{
				highlightSprite = highlightBtnsSprites[i];
			}
			else if(highlightBtnsSprites.Count > 0)
			{
				highlightSprite = highlightBtnsSprites[0];
			}
			if(highlightSprite != null ) newResponse.Btn.image.sprite = highlightSprite;
			for (int j = 0; j < users.Length; j++) users[j].OnShowResponseBtn(newResponse);
		}
		gameObject.SetActive(true);
	}

	public void ActiveConfirmationButton(bool value)
	{
		confirmationButton.gameObject.SetActive(value);
	}

	public void SetCanInteractWithBtns(bool value)
	{
		for (int i = 0; i < currResponses.Count; i++)
		{
			currResponses[i].SetRaycastInteractable(value);
		}
	}

	public void GrayOutResponse(int responseIdx)
	{
		currResponses[responseIdx].Btn.interactable = false;
	}

	public bool HasAvailableResponse()
	{
		for (int i = 0; i < currResponses.Count; i++)
		{
			if (currResponses[i].Btn.interactable) return true;
		}
		return false;
	}


	public void HighlightResponse(DialogueResponse response)
	{
		for (int i = 0; i < currResponses.Count; i++)
		{
			var curr = currResponses[i];
			if (currResponses[i].ResponseData == response)
			{
				curr.transform.localScale = Vector3.one * 1.1f;
				currHighlightedResponse = curr;
				Sprite highlightSprite = null;
				if (i < highlightBtnsSprites.Count)
				{
					highlightSprite = highlightBtnsSprites[i];
				}
				else if (highlightBtnsSprites.Count > 0)
				{
					highlightSprite = highlightBtnsSprites[0];
				}
				if (highlightSprite) curr.Btn.image.sprite = highlightSprite;
			}
			else UnClickResponse(curr);
		}
	}

	public void OnClickedResponse(DialogueResponse response)
	{
		HighlightResponse(response);
		if (UseConfirmationBtn) mainUi.OnClickResponse(response);
		else
		{
			mainUi.OnClickResponse(response);
			mainUi.preselectedResponseAudioIsDone = true;
			mainUi.OnClickResponseConfirmation();
		}

		if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.stepResponseButton))
		{
			TutorialManager.Instance.TurnOffTutorialStep(tutorialSteps.stepResponseButton);
		}
	}

	public void UnClickResponse(ResponseBtn response)
	{
        response.transform.localScale = Vector3.one;
        if (disableBtnContainer) response.Btn.image.sprite = disableBtnContainer;
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
