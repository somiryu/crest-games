using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueBubbleResponseDisplayerUI : MonoBehaviour, IDialoguesResponseDisplayerUser
{
    DialoguesResponsesDisplayerUI mainDisplayer;

	//Posibly will have to move this to a world Position
	public Transform pivotObj;
	public List<Transform> btnHolders;

	int currBtnIdxBeingPlaced = 0;

	public void Init(DialoguesResponsesDisplayerUI _mainUi)
	{
		mainDisplayer = _mainUi;
	}

	public void OnShowResponses(DialogueResponse[] responseDatas)
	{
		currBtnIdxBeingPlaced = 0;
	}

	public void OnShowResponseBtn(ResponseBtn responseBtn)
	{
		if (currBtnIdxBeingPlaced >= btnHolders.Count) Debug.LogError("Trying to show more btn than there's positions holders for them");
		responseBtn.RectTransform.SetParent(btnHolders[currBtnIdxBeingPlaced]);
		responseBtn.RectTransform.anchoredPosition = Vector2.zero;
		currBtnIdxBeingPlaced++;

	}
}
