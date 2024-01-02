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
    [SerializeField] Image tutorialImg;


    private DialogueResponse responseData;
	public Button Btn => btn;
    public TMP_Text ResponseTxt => responseTxt;
    public DialogueResponse ResponseData => responseData;

	public Action<DialogueResponse> onClicked;

	public RectTransform RectTransform;



	private void Awake()
	{
		TryGetComponent(out RectTransform);
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => onClicked?.Invoke(ResponseData));
       
	}

	public void SetData(DialogueResponse _responseData)
	{
        responseData = _responseData;

		if (responseTxt != null)
		{
            if (!string.IsNullOrEmpty(responseData.responseAlternative))
            {
                var userGender = UserDataManager.CurrUser.gender;
                var currResponse = (userGender == UserGender.Femenino) ? responseData.responseAlternative : responseData.response;
                SetResponseText(currResponse);
            }
            else
            {
                SetResponseText(responseData.response);
            }
        }
        if (responseImg != null)
		{
			responseImg.gameObject.SetActive(responseData.responseImage != null);
			responseImg.sprite = responseData.responseImage;
		}
        if(!UserDataManager.CurrUser.tutorialNarrative) { 
            tutorialImg.gameObject.SetActive(true);
        }

		onClicked = null;
		btn.interactable = true;
	}

    void SetResponseText(string text)
    {
        responseTxt.SetText(text);
        responseTxt.gameObject.SetActive(!string.IsNullOrEmpty(text));
    }


}
