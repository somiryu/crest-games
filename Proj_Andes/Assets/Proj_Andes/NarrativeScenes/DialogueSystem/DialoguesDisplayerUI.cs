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
    private static DialoguesDisplayerUI instance;
    public static DialoguesDisplayerUI Instance => instance;


    [SerializeField] DialogueSequenceData dialoguesToShow;
    public DialogueSequenceData CurrDialoguesBeingShown => dialoguesToShow;
    [Space(20)]
    [SerializeField] GameObject mainDialoguesGraphics;
    [SerializeField] Image characterImage;
    [SerializeField] GameObject characterImageContainer;
    [SerializeField] TMP_Text nameTxt;
    [SerializeField] GameObject nameTxtContainer;
    [SerializeField] TMP_Text dialogueTxt;
    [SerializeField] GameObject dialogueTxtContainer;
    [SerializeField] Button skipDialogueBtn;
    [SerializeField] Button dialogueBoxBtn;
    [SerializeField] Button repeatBtn;
    [SerializeField] PlayableDirector timeLinePlayer;
    [SerializeField] Transform responseDisplayersContainer;
    [SerializeField] AudioSource audioPlayer;

    [SerializeField] bool forceDialogeAppear;

    private int currShowingIdx = -1;
    private bool isShowing = false;
    private bool hasPendingLineChange = false;
    private bool audioIsDone = false;
    private DialogueSequenceData pendingSequenceToShow;
    public bool IsShowing => isShowing;
    public dialogLineState state = dialogLineState.NotShowing;

    //Appear dialogue params
    private bool isAppearingTxt = false;
    private bool forceEndAppearingTxt = false;
    private float appearTimer = 0;
    private float appearTime = 2;
    private int currCharProgress;
    private char[] currDialogueCharacters;

    private DialoguesResponsesDisplayerUI currResponsesDisplayer;

    //This stores a reference to the original prefab linked to the cached instance, so that we don't instantiate two of the same.
    private Dictionary<DialoguesResponsesDisplayerUI, DialoguesResponsesDisplayerUI> cachedResponseDisplayers
        = new Dictionary<DialoguesResponsesDisplayerUI, DialoguesResponsesDisplayerUI>();


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
        if(instance != null && instance != this) DestroyImmediate(instance);
        instance = this;

        skipDialogueBtn.onClick.AddListener(OnDialogueBoxBtnPressed);
        dialogueBoxBtn.onClick.AddListener(OnDialogueBoxBtnPressed);
        repeatBtn.onClick.AddListener(() => ShowCurrDialog(true));
    }

    public bool OnWantsToChangeDialogFromTrigger()
    {
        if (!audioIsDone) return false;
        if (state != dialogLineState.Idle) return false;
        
        if (isAppearingTxt)
        {
            forceEndAppearingTxt = true;
        }
        else
        {
            //We want to wait until the exit anim is done, if there's one, that's way there's no inmediate change in here
            hasPendingLineChange = true;
        }

        return true;
	}

    private void OnDialogueBoxBtnPressed()
	{
		if (isAppearingTxt)
		{
            forceEndAppearingTxt = true;
		}
		else
		{
            if (AutoContinueActive())
            {
                //We want to wait until the exit anim is done, if there's one, that's way there's no inmediate change in here
                hasPendingLineChange = true;
            }
		}
	}

    bool AutoContinueActive()
    {
		var currDialogue = dialoguesToShow.dialogues[currShowingIdx];
        return currDialogue.autoContinueOnClickDialog && currDialogue.responses.Length == 0;
    }

    /// <summary>
    /// Set the new sequence as pending to be shown, (Not changing directly because we want to wait for the exit anim to happen)
    /// </summary>
    public bool SetPendingSequence(DialogueSequenceData newDialogSequence)
    {
		if (!audioIsDone) return false;
		if (state != dialogLineState.Idle) return false;

		pendingSequenceToShow = newDialogSequence;
        hasPendingLineChange = true;

        return true;
    }


    public void ShowDialogueSequence(DialogueSequenceData newDialogues) {
        pendingSequenceToShow = null;
        dialoguesToShow = newDialogues;
        currShowingIdx = -1;
        isShowing = true;
        appearTime = DialogueConfigs.Instace.appearTime;
        mainDialoguesGraphics.SetActive(true);
        NextDialogue();
        OnStartShowingDialogue?.Invoke();
    }

    public void NextDialogue() {

        DialogueData lastPlayedDialog = null;
        if(currShowingIdx > -1 && currShowingIdx < dialoguesToShow.dialogues.Length) lastPlayedDialog = dialoguesToShow.dialogues[currShowingIdx];

        currShowingIdx++;
        if(dialoguesToShow == null || currShowingIdx >= dialoguesToShow.dialogues.Length) {
            DialogueSequenceData nextSequence = null;
            if(dialoguesToShow != null)
            {
                nextSequence = lastPlayedDialog.changeToSequence;
            }
            HideDialogues();
            if(nextSequence != null)
            {
                ShowDialogueSequence(nextSequence);
            }
            return;
        }
        ShowCurrDialog();
    }

    private void ShowCurrDialog(bool SkipEnterAnim = false)
    {
		var curr = dialoguesToShow.dialogues[currShowingIdx];

		repeatBtn.gameObject.SetActive(false);
		skipDialogueBtn.gameObject.SetActive(false);

		var currResponses = curr.responses;
		//Clean old responses if needed
		if (currResponsesDisplayer != null) currResponsesDisplayer.Hide();
		//Get new response handler
		currResponsesDisplayer = GetResponseDisplayer(curr);
		if (currResponsesDisplayer != null) currResponsesDisplayer.ShowResponses(currResponses);

        //Image and name of character
		var currCharConfigs = curr.characterType.GetCharacterConfig();
		characterImageContainer.SetActive(currCharConfigs.image != null);
		characterImage.sprite = currCharConfigs.image;
		nameTxtContainer.SetActive(!string.IsNullOrEmpty(currCharConfigs.name));
		nameTxt.SetText(currCharConfigs.name);


		currDialogueCharacters = curr.text.ToCharArray();
		dialogueTxtContainer.SetActive(currDialogueCharacters.Length > 0);
		dialogueTxt.SetText("");

		if (currAnimSequence != null) StopCoroutine(currAnimSequence);
		currAnimSequence = DialogAnimSequence(curr, SkipEnterAnim);
		StartCoroutine(currAnimSequence);
	}

    private DialoguesResponsesDisplayerUI GetResponseDisplayer(DialogueData dialogueData)
    {
        if(dialogueData.responses.Length == 0) return null;
        if(dialogueData.responsesDisplayerPrefab == null)
        {
            Debug.LogError("You have responses, but you haven't set a response displayer on dialogue data" + dialogueData.responses[0].response, CurrDialoguesBeingShown);
            return null;
        }
        //Already cached
        if(cachedResponseDisplayers.TryGetValue(dialogueData.responsesDisplayerPrefab, out var responsesDisplayerUI))
        {
            return responsesDisplayerUI;
        }
        //It's a new one
        var instance = Instantiate(dialogueData.responsesDisplayerPrefab, responseDisplayersContainer);
        instance.Init(this);
        cachedResponseDisplayers.Add(dialogueData.responsesDisplayerPrefab, instance);
        return instance;
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

        if (isAppearingTxt) AppearText();
    }

    private IEnumerator currAnimSequence;

    private IEnumerator DialogAnimSequence(DialogueData dialogueData, bool skipEnterAnim = false)
    {
        state = dialogLineState.Entering;
        if (dialogueData.EnterAnim != null && !skipEnterAnim)
        {
            timeLinePlayer.extrapolationMode = DirectorWrapMode.None;
			timeLinePlayer.playableAsset = dialogueData.EnterAnim;
            timeLinePlayer.Play();
            while(timeLinePlayer.state == PlayState.Playing) yield return null;
        }

		audioIsDone = true;
		//Start playing audio
		if (dialogueData.audio != null)
		{
			audioPlayer.clip = dialogueData.audio;
			audioPlayer.Play();
			audioIsDone = false;
		}

        //Start showing text
		if (currDialogueCharacters.Length > 0)
		{
			currCharProgress = 0;
			StartTextAppear();
			currText.Clear();
			currText.Append(currDialogueCharacters[currCharProgress]);
		}

		state = dialogLineState.Idle;
        if(dialogueData.IdleAnim != null)
        {
			timeLinePlayer.extrapolationMode = DirectorWrapMode.Loop;
			timeLinePlayer.playableAsset = dialogueData.IdleAnim;
            timeLinePlayer.Play();
        }

		while (!audioIsDone)
        {
            audioIsDone = !audioPlayer.isPlaying;
            yield return null;
        }

		repeatBtn.gameObject.SetActive(!string.IsNullOrEmpty(dialogueData.text) || dialogueData.audio != null);


		while (!hasPendingLineChange) yield return null;


		state = dialogLineState.Exiting;
        if(dialogueData.ExitAnim != null)
        {
			timeLinePlayer.extrapolationMode = DirectorWrapMode.None;
			timeLinePlayer.playableAsset = dialogueData.ExitAnim;
            timeLinePlayer.Play();
			while (timeLinePlayer.state == PlayState.Playing) yield return null;
		}
        state = dialogLineState.NotShowing;
        hasPendingLineChange = false;
        currAnimSequence = null;

        if (pendingSequenceToShow != null) ShowDialogueSequence(pendingSequenceToShow);
        else NextDialogue();

	}


    public void OnClickResponse(DialogueResponse responseClicked)
	{
		if (responseClicked.changeSequence)
		{
            pendingSequenceToShow = responseClicked.dataAfterResponse;
            hasPendingLineChange = true;
		}
		else hasPendingLineChange = true;
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
            var turnOnAutoSkip = AutoContinueActive();
            skipDialogueBtn.gameObject.SetActive(turnOnAutoSkip);
            dialogueBoxBtn.gameObject.SetActive(turnOnAutoSkip);
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
