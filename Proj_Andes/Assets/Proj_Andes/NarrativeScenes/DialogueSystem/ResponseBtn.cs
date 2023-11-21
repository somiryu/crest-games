using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ResponseBtn : MonoBehaviour
{
    [SerializeField]
    private Button btn;
    public Button Btn => btn;

    [SerializeField]
    private TMP_Text responseTxt;
    public TMP_Text ResponseTxt => responseTxt;

    private DialogueResponse responseData;
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
        responseTxt.SetText(responseData.response);
        onClicked = null
            ;
	}
}
