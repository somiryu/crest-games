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
    [SerializeField] PlayableDirector timeLinePlayer;
    [SerializeField] Transform responseDisplayersContainer;

    [SerializeField] bool forceDialogeAppear;

    private int currShowingIdx = -1;
    private bool isShowing = false;
    private bool hasPendingDialogueChange = false;
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

        skipDialogueBtn.onClick.AddListener(NextDialogue);
        dialogueBoxBtn.onClick.AddListener(OnDialogueBoxBtnPressed);
    }

    public void OnWantsToChangeDialogFromTrigger()
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
                hasPendingDialogueChange = true;
            }
		}
	}

    bool AutoContinueActive()
    {
		var currDialogue = dialoguesToShow.dialogues[currShowingIdx];
        return currDialogue.autoContinueOnClickDialog && currDialogue.responses.Length == 0;
    }


    public void ShowDialogueSequence(DialogueSequenceData newDialogues) {
        dialoguesToShow = newDialogues;
        currShowingIdx = -1;
        isShowing = true;
        appearTime = DialogueConfigs.Instace.appearTime;
        mainDialoguesGraphics.SetActive(true);
        NextDialogue();
        OnStartShowingDialogue?.Invoke();
    }

    public void NextDialogue() {
        currShowingIdx++;
        if(dialoguesToShow == null || currShowingIdx >= dialoguesToShow.dialogues.Length) {
            HideDialogues();
            return;
        }
        var curr = dialoguesToShow.dialogues[currShowingIdx];
        var currCharConfigs = curr.characterType.GetCharacterConfig();
        var currResponses = curr.responses;

        //Clean old responses if needed
        if (currResponsesDisplayer != null) currResponsesDisplayer.Hide();
		//Get new response handler
		currResponsesDisplayer = GetResponseDisplayer(curr);
        if(currResponsesDisplayer != null) currResponsesDisplayer.ShowResponses(currResponses);

        characterImageContainer.SetActive(currCharConfigs.image != null);
        characterImage.sprite = currCharConfigs.image;

        nameTxtContainer.SetActive(string.IsNullOrEmpty(currCharConfigs.name));
        nameTxt.SetText(currCharConfigs.name);

        currDialogueCharacters = curr.text.ToCharArray();

        if(currDialogueCharacters.Length > 0 )
        {
			dialogueTxtContainer.SetActive(true);
			currCharProgress = 0;
			StartTextAppear();
			currText.Clear();
			currText.Append(currDialogueCharacters[currCharProgress]);
		}
		else dialogueTxtContainer.SetActive(false);

       
        skipDialogueBtn.gameObject.SetActive(false);

        if(currAnimSequence != null) StopCoroutine(currAnimSequence);
        currAnimSequence = DialogAnimSequence(curr);
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


    public void OnClickResponse(DialogueResponse responseClicked)
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
