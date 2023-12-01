using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ResponseBtn : MonoBehaviour
{
	[SerializeField] Button btn;
	[SerializeField] TMP_Text responseTxt;
	[SerializeField] Image responseImg;

    private DialogueResponse responseData;
	public Button Btn => btn;
    public TMP_Text ResponseTxt => responseTxt;
    public DialogueResponse ResponseData => responseData;

	public Action<DialogueResponse> onClicked;

	private void Awake()
	{
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClicked?.Invoke(ResponseData));
	}

	public void SetData(DialogueResponse _responseData)
	{
        responseData = _responseData;

		if (responseTxt != null)
		{
			responseTxt.SetText(responseData.response);
			responseTxt.gameObject.SetActive(!string.IsNullOrEmpty(responseData.response));
		}
		if(responseImg != null)
		{
			responseImg.gameObject.SetActive(responseData.responseImage != null);
			responseImg.sprite = responseData.responseImage;
		}
		onClicked = null;
	}
}
