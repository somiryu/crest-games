using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using System;
using UnityEngine.Playables;
using UnityEditor.Timeline;

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
    [SerializeField] PlayableDirector timeLinePlayer;

    [SerializeField] Pool<ResponseBtn> responseBtnsPool;
    [SerializeField] bool forceDialogeAppear;

    private int currShowingIdx = -1;
    private bool isShowing = false;
    private bool hasPendingDialogueChange = false;
    public bool IsShowing => isShowing;
    private KeyCode skipKey = KeyCode.Space;
    public dialogLineState state = dialogLineState.NotShowing;

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
                ShowDialogueSequence(dialoguesToShow);
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
            //We want to wait until the exit anim is done, if there's one, that's way there's no inmediate change in here
            hasPendingDialogueChange = true;
		}
	}


    public void ShowDialogueSequence(DialogueSequenceData newDialogues) {
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

        if(currAnimSequence != null) StopCoroutine(currAnimSequence);
        currAnimSequence = DialogAnimSequence(curr);
        StartCoroutine(currAnimSequence);
    }

    public void StartTextAppear()
	{
        isAppearingTxt = true;
        dialogueBoxBtn.gameObject.SetActive(true);
        forceEndAppearingTxt = false;
    }

    public void Update()
    {
        if (!isShowing) return;
        if (Input.GetKeyDown(KeyCode.I))
        {
            HideDialogues();
            return;
        }
        if (isAppearingTxt)
        {
            AppearText();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnDialogueBoxBtnPressed();
        }
    }

    private IEnumerator currAnimSequence;

    private IEnumerator DialogAnimSequence(DialogueData dialogueData)
    {
        state = dialogLineState.Entering;
        if (dialogueData.EnterAnim != null)
        {
            timeLinePlayer.extrapolationMode = DirectorWrapMode.None;
			timeLinePlayer.playableAsset = dialogueData.EnterAnim;
            timeLinePlayer.Play();
            while(timeLinePlayer.state == PlayState.Playing) yield return null;
        }

        state = dialogLineState.Idle;
        if(dialogueData.IdleAnim != null)
        {
			timeLinePlayer.extrapolationMode = DirectorWrapMode.Loop;
			timeLinePlayer.playableAsset = dialogueData.IdleAnim;
            timeLinePlayer.Play();
            while(!hasPendingDialogueChange) yield return null;
        }

        state = dialogLineState.Exiting;
        if(dialogueData.ExitAnim != null)
        {
			timeLinePlayer.extrapolationMode = DirectorWrapMode.None;
			timeLinePlayer.playableAsset = dialogueData.ExitAnim;
            timeLinePlayer.Play();
			while (timeLinePlayer.state == PlayState.Playing) yield return null;
		}
        state = dialogLineState.NotShowing;
        hasPendingDialogueChange = false;
        currAnimSequence = null;
        NextDialogue();
	}


    private void OnClickResponse(DialogueResponse responseClicked)
	{
		if (responseClicked.changeSequence)
		{
            ShowDialogueSequence(responseClicked.dataAfterResponse);
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
        currShowingIdx = -1;

	}
}

public enum dialogLineState
{
    NotShowing,
    Entering,
    Idle,
    Exiting
}
