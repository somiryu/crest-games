using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using System;

public class DialoguesDisplayerUI : MonoBehaviour
{
    [SerializeField] DialogueSequenceData dialoguesToShow;
    public DialogueSequenceData CurrDialoguesBeingShown => dialoguesToShow;
    [Space(20)]
    [SerializeField] GameObject mainDialoguesGraphics;
    [SerializeField] Image characterImage;
    [SerializeField] GameObject characterImageContainer;
    [SerializeField] TMP_Text nameTxt;
    [SerializeField] GameObject nameTxtContainer;
    [SerializeField] TMP_Text dialogueTxt;
    [SerializeField] Button skipDialogueBtn;
    [SerializeField] Button dialogueBoxBtn;

    [SerializeField] Pool<ResponseBtn> responseBtnsPool;
    [SerializeField] bool forceDialogeAppear;

    private int currShowingIdx = -1;
    private bool isShowing = false;
    public bool IsShowing => isShowing;
    private KeyCode skipKey = KeyCode.Space;

    //Appear dialogue params
    private bool isAppearingTxt = false;
    private bool forceEndAppearingTxt = false;
    private float appearTimer = 0;
    private float appearTime = 2;
    private int currCharProgress;
    private char[] currDialogueCharacters;

    private StringBuilder currText = new StringBuilder();

    public Action OnStartShowingDialogue;
    public Action OnEndShowingDialogue;

    private void OnValidate() {
        if (forceDialogeAppear) {
            if (Application.isPlaying) {
                ShowDialogue(dialoguesToShow);
            }
            forceDialogeAppear = false;
        }
    }

    private void Awake() {
        responseBtnsPool.Init(4);
        skipDialogueBtn.onClick.AddListener(NextDialogue);
        dialogueBoxBtn.onClick.AddListener(OnDialogueBoxBtnPressed);
    }

    private void OnDialogueBoxBtnPressed()
	{
		if (isAppearingTxt)
		{
            forceEndAppearingTxt = true;
		}
		else
		{
            NextDialogue();
		}
	}


    public void ShowDialogue(DialogueSequenceData newDialogues) {
        dialoguesToShow = newDialogues;
        currShowingIdx = -1;
        isShowing = true;
        skipKey = DialogueConfigs.Instace.skipKey;
        appearTime = DialogueConfigs.Instace.appearTime;
        mainDialoguesGraphics.SetActive(true);
        NextDialogue();
        OnStartShowingDialogue?.Invoke();
    }

    public void NextDialogue() {
        currShowingIdx++;
        if(currShowingIdx >= dialoguesToShow.dialogues.Length) {
            HideDialogues();
            return;
        }
        var curr = dialoguesToShow.dialogues[currShowingIdx];
        var currCharConfigs = curr.characterType.GetCharacterConfig();
        var currResponses = curr.responses;
        responseBtnsPool.RecycleAll();
		for (int i = 0; i < currResponses.Length; i++)
		{
            var currResponse = currResponses[i];
            var responseBtn = responseBtnsPool.GetNewItem();
            responseBtn.SetData(currResponse);
            responseBtn.onClicked += OnClickResponse;
		}

        characterImageContainer.SetActive(currCharConfigs.image != null);
        characterImage.sprite = currCharConfigs.image;

        nameTxtContainer.SetActive(string.IsNullOrEmpty(currCharConfigs.name));
        nameTxt.SetText(currCharConfigs.name);

        currDialogueCharacters = curr.text.ToCharArray();
        currCharProgress = 0;
        StartTextAppear();
        currText.Clear();
        currText.Append(currDialogueCharacters[currCharProgress]);
        skipDialogueBtn.gameObject.SetActive(false);
    }

    public void StartTextAppear()
	{
        isAppearingTxt = true;
        dialogueBoxBtn.gameObject.SetActive(true);
        forceEndAppearingTxt = false;
    }

    public void Update() {
        if (!isShowing) return;
		if (Input.GetKeyDown(KeyCode.I))
		{
            HideDialogues();
            return;
		}
        if (isAppearingTxt) {
            AppearText();
        }
		if (Input.GetKeyDown(KeyCode.Space))
		{
            OnDialogueBoxBtnPressed();
        }
    }

    private void OnClickResponse(DialogueResponse responseClicked)
	{
		if (responseClicked.changeSequence)
		{
            ShowDialogue(responseClicked.dataAfterResponse);
		}
		else
		{
            NextDialogue();
		}
	}

    public void AppearText() {
        var currDialogue = dialoguesToShow.dialogues[currShowingIdx];
        dialogueTxt.SetText(currText.ToString());
        appearTimer += Time.deltaTime;
        if (appearTimer >= appearTime) {
            appearTimer = 0;
            currCharProgress++;
            if (currCharProgress >= currDialogueCharacters.Length) {
                forceEndAppearingTxt = true;
            } else {
                currText.Append(currDialogueCharacters[currCharProgress]);
            }
        }
       
        if (forceEndAppearingTxt) {
            isAppearingTxt = false;
            dialogueTxt.SetText(currDialogue.text);
            skipDialogueBtn.gameObject.SetActive(currDialogue.responses.Length == 0);
            dialogueBoxBtn.gameObject.SetActive(currDialogue.responses.Length == 0);
        }
    }

    public void HideDialogues() {
        isAppearingTxt = false;
        isShowing = false;
        dialoguesToShow = null;
        mainDialoguesGraphics.SetActive(false);
        OnEndShowingDialogue?.Invoke();
    }
}
