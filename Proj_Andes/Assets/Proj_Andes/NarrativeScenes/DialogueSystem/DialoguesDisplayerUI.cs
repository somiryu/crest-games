using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;
using System;
using UnityEngine.Playables;
using Unity.VisualScripting;
using Firebase.Firestore;
using UnityEngine.Serialization;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class DialoguesDisplayerUI : MonoBehaviour
{
    private static DialoguesDisplayerUI instance;
    public static DialoguesDisplayerUI Instance => instance;
    public NarrativeSequenceItem narrativeSceneItem;

    public DialogueSequenceData dialoguesToShow;
    public DialogueSequenceData CurrDialoguesBeingShown => dialoguesToShow;
    [Space(20)]
    [SerializeField] GameObject mainDialoguesGraphics;
    [SerializeField] Image characterImage;
    [SerializeField] GameObject characterImageContainer;
    [SerializeField] TMP_Text nameTxt;
    [SerializeField] GameObject nameTxtContainer;
    [SerializeField] TMP_Text dialogueTxt;
    [SerializeField] GameObject dialogueTxtContainer;
    [SerializeField] Image skipDialogueTutImg;
    [FormerlySerializedAs("dialogueBoxBtn")]
    [SerializeField] Button fullScreenInvisibleBtn;
    [SerializeField] Button repeatBtn;
    [SerializeField] PlayableDirector timeLinePlayer;
    [SerializeField] Transform responseDisplayersContainer;
    [SerializeField] AudioSource audioPlayer;
    [SerializeField] Toggle canSkipAudio;
    [SerializeField] Button skipSceneBtn;
    [SerializeField] bool activeSkipSceneBtn;
    [SerializeField] Button continueBtn;
    bool hasResponse;

    [SerializeField] bool forceDialogeAppear;

    private int currShowingIdx = -1;
    private int lastDisplayedDialogLineIdx = -1;
    private int lastPickedResponseIdx = -1;
    private bool isShowing = false;
    private bool hasPendingLineChange = false;
    private bool audioIsDone = false;
    private DialogueSequenceData pendingSequenceToShow;
    private DialogueResponse preselectedResponse;
    [NonSerialized] public bool preselectedResponseAudioIsDone = false;


    public bool SaveNavSequence = true;
    public bool doneResponsePreview;


    public bool IsShowing => isShowing;
    public dialogLineState state = dialogLineState.NotShowing;

    //Appear dialogue params
    private bool isAppearingTxt = false;
    private bool forceEndAppearingTxt = false;
    private float appearTimer = 0;
    private float appearTime = 2;
    private int currCharProgress;
    private char[] currDialogueCharacters;

    //Analytics
    private float currResponseTime;
    private string currResponseChoiceAnalyticIDTm;
    private string currResponseChoiceAnalyticIDCod;
    private string currResponseChoiceAnalyticIDRta;
    private int analyticsCount;
    private string currResponseAnalyticResponse;
    private float currResponseAnalyticResponseVal;

    private DialoguesResponsesDisplayerUI currResponsesDisplayer;

    //This stores a reference to the original prefab linked to the cached instance, so that we don't instantiate two of the same.
    private Dictionary<DialoguesResponsesDisplayerUI, DialoguesResponsesDisplayerUI> cachedResponseDisplayers
        = new Dictionary<DialoguesResponsesDisplayerUI, DialoguesResponsesDisplayerUI>();


    private StringBuilder currText = new StringBuilder();

    public DialogueData CurrDialog => dialoguesToShow.dialogues[currShowingIdx];
    public int CurrDialogIdx => currShowingIdx;

    public Action OnStartShowingDialogue;
    public Action OnEndShowingDialogue;

    [NonSerialized]
    public List<NarrativeNavigationNode> choicesTree = new List<NarrativeNavigationNode>();

    /// <summary>
    /// The user data manager writes this values if it detects that there's a saved narrativeindex that we should push
    /// </summary>
    public static List<NarrativeNavigationNode> CheckPointTreeToConsume;

    private void OnValidate()
    {
        if (forceDialogeAppear)
        {
            if (Application.isPlaying)
            {
                ShowDialogueSequence(dialoguesToShow);
            }
            forceDialogeAppear = false;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this) DestroyImmediate(instance);
        instance = this;
        canSkipAudio.isOn = false;

        continueBtn.onClick.AddListener(OnClickToContinueBtn);
        hasResponse = true;
        fullScreenInvisibleBtn.onClick.AddListener(OnFullScreenInvisibleBtnClicked);
        repeatBtn.onClick.AddListener(() => ShowCurrDialog(true));
        choicesTree.Clear();
        skipSceneBtn.gameObject.SetActive(activeSkipSceneBtn && !AppSkipSceneButton.ActiveDebugGlobalUI);
        skipSceneBtn.onClick.AddListener(GameSequencesList.Instance.GoToNextItemInList);

        if (!canSkipAudio.gameObject.activeSelf) canSkipAudio.isOn = false;

        narrativeSceneItem.ResetCurrentAnalytics();
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

    private void OnFullScreenInvisibleBtnClicked()
    {

        if (isAppearingTxt)
        {
            forceEndAppearingTxt = true;
        }
    }

    private void OnClickToContinueBtn()
    {
        if (AutoContinueActive() && audioIsDone)
        {
            //We want to wait until the exit anim is done, if there's one, that's way there's no inmediate change in here
            hasPendingLineChange = true;
            if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.stepSkipButton))
            {
                TutorialManager.Instance.TurnOffTutorialStep(tutorialSteps.stepSkipButton);
            }
        }
    }

    bool AutoContinueActive()
    {
        if (currShowingIdx == -1) return false;
        var currDialogue = dialoguesToShow.dialogues[currShowingIdx];
        return currDialogue.autoContinueOnClickDialog &&
            (currDialogue.responses.Length == 0 || currDialogue.AllResponsesWereGrayOut(grayOutResponseIdxes));
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

    int customStartIdx = -1;
    List<int> grayOutResponseIdxes = new List<int>();

    public void ShowDialogueSequence(DialogueSequenceData newDialogues)
    {
        if (lastDisplayedDialogLineIdx != -1)
        {
            var narrNavigation = new NarrativeNavigationNode(
                       _sourceDialogIdx: lastDisplayedDialogLineIdx,
                       _responsePickedIdx: lastPickedResponseIdx,
                       _nextDialogCustomStartIdx: customStartIdx);

            Debug.Log("Registering new narr navitagation from dialogLine idx: " + narrNavigation.sourceDialogIdx);
            Debug.Log("Response picked: " + narrNavigation.responsePickedIdx);
            Debug.Log("Next dialog custom start idx: " + narrNavigation.nextDialogCustomStartIdx);

            choicesTree.Add(narrNavigation);
        }
        //This is the first secuence of this scene, check if we have a pending checkpoint info
        else if (CheckPointTreeToConsume != null && CheckPointTreeToConsume.Count > 0)
        {
            newDialogues = LoadCheckPointInfo(newDialogues);
        }

        pendingSequenceToShow = null;
        dialoguesToShow = newDialogues;
        currShowingIdx = customStartIdx;
        customStartIdx = -1;
        isShowing = true;
        appearTime = DialogueConfigs.Instace.appearTime;
        mainDialoguesGraphics.SetActive(true);
        NextDialogue();
        OnStartShowingDialogue?.Invoke();
    }


    DialogueSequenceData LoadCheckPointInfo(DialogueSequenceData startSequence)
    {
        var currSequence = startSequence;
        for (int i = 0; i < CheckPointTreeToConsume.Count - 1; i++)
        {
            currSequence = GetTargetSequence(currSequence, CheckPointTreeToConsume[i]);
        }
        //-1 as we will call the nextDialogue after this
        customStartIdx = CheckPointTreeToConsume[CheckPointTreeToConsume.Count - 1].sourceDialogIdx - 1;
        choicesTree = new List<NarrativeNavigationNode>(CheckPointTreeToConsume);
        //The last position was an unfinished movement, so remove that one
        choicesTree.RemoveAt(choicesTree.Count - 1);
        CheckPointTreeToConsume.Clear();
        return currSequence;
    }
    DialogueSequenceData GetTargetSequence(DialogueSequenceData startSequence, NarrativeNavigationNode navInfo)
    {
        var fromDialogLine = startSequence.dialogues[navInfo.sourceDialogIdx];
        if (navInfo.responsePickedIdx != -1)
        {
            var targetResponse = fromDialogLine.responses[navInfo.responsePickedIdx];
            if (targetResponse.dataAfterResponse != null) return targetResponse.dataAfterResponse;
            else return fromDialogLine.changeToSequence;
        }
        else return fromDialogLine.changeToSequence;
    }

    public void NextDialogue()
    {

        DialogueData lastPlayedDialog = null;
        if (currShowingIdx > -1 && currShowingIdx < dialoguesToShow.dialogues.Length) lastPlayedDialog = dialoguesToShow.dialogues[currShowingIdx];

        currShowingIdx++;
        if (dialoguesToShow == null || currShowingIdx >= dialoguesToShow.dialogues.Length)
        {
            DialogueSequenceData nextSequence = null;
            customStartIdx = -1;
            if (dialoguesToShow != null)
            {
                nextSequence = lastPlayedDialog.changeToSequence;
                if (lastPlayedDialog.changeToSequenceStartDialogIdx != -1)
                {
                    customStartIdx = lastPlayedDialog.changeToSequenceStartDialogIdx - 1; //Minus one because we call "Next dialogue" after this
                }
                if (lastPlayedDialog.changeToSequenceResponseIdxToGrayOut != -1)
                {
                    if (!grayOutResponseIdxes.Contains(lastPlayedDialog.changeToSequenceResponseIdxToGrayOut))
                    {
                        grayOutResponseIdxes.Add(lastPlayedDialog.changeToSequenceResponseIdxToGrayOut);
                    }
                }
            }
            HideDialogues(nextSequence == null);
            if (nextSequence != null)
            {
                ShowDialogueSequence(nextSequence);
            }
            return;
        }
        ShowCurrDialog();
    }

    private void ShowCurrDialog(bool SkipEnterAnim = false)
    {
        lastDisplayedDialogLineIdx = currShowingIdx;
        var curr = dialoguesToShow.dialogues[currShowingIdx];

        if (CurrDialog.AllResponsesWereGrayOut(grayOutResponseIdxes))
        {
            grayOutResponseIdxes.Clear();
            ShowDialogueSequence(CurrDialog.changeToSequence);
            return;
        }

        repeatBtn.gameObject.SetActive(false);
        skipDialogueTutImg.gameObject.SetActive(false);

        //Clean old responses if needed
        if (currResponsesDisplayer != null) currResponsesDisplayer.Hide();


        //Image and name of character
        var currCharConfigs = curr.characterType.GetCharacterConfig();
        characterImageContainer.SetActive(currCharConfigs.image != null);
        characterImage.sprite = currCharConfigs.image;
        nameTxtContainer.SetActive(!string.IsNullOrEmpty(currCharConfigs.name));
        nameTxt.SetText(currCharConfigs.name);

        currDialogueCharacters = SelectTextByGender(curr).ToCharArray();

        dialogueTxtContainer.SetActive(currDialogueCharacters.Length > 0);
        dialogueTxt.SetText("");

        if (currAnimSequence != null) StopCoroutine(currAnimSequence);
        currAnimSequence = DialogAnimSequence(curr, SkipEnterAnim);
        StartCoroutine(currAnimSequence);
    }

    private DialoguesResponsesDisplayerUI GetResponseDisplayer(DialogueData dialogueData)
    {
        if (dialogueData.responses.Length == 0) return null;
        if (dialogueData.responsesDisplayerPrefab == null)
        {
            Debug.LogError("You have responses, but you haven't set a response displayer on dialogue data" + dialogueData.responses[0].response, CurrDialoguesBeingShown);
            return null;
        }
        //Already cached
        if (cachedResponseDisplayers.TryGetValue(dialogueData.responsesDisplayerPrefab, out var responsesDisplayerUI))
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
        fullScreenInvisibleBtn.gameObject.SetActive(true);
        forceEndAppearingTxt = false;
    }
    public void Update()
    {
        if (!isShowing) return;
        if (Input.GetKeyDown(KeyCode.I))
        {
            HideDialogues(false);
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
            while (timeLinePlayer.state == PlayState.Playing) yield return null;
        }

        audioIsDone = true;
        //Start playing audio
        var audio = SelectAudioByGender(dialogueData);

        continueBtn.gameObject.SetActive(false);


        if (audio != null)
        {
            audioPlayer.clip = audio;
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
        if (dialogueData.IdleAnim != null)
        {
            timeLinePlayer.extrapolationMode = DirectorWrapMode.Loop;
            timeLinePlayer.playableAsset = dialogueData.IdleAnim;
            timeLinePlayer.Play();
        }

        while (!audioIsDone)
        {
            audioIsDone = !audioPlayer.isPlaying;
            if (canSkipAudio.isOn) audioIsDone = true;
            yield return null;
        }


        repeatBtn.gameObject.SetActive(!string.IsNullOrEmpty(dialogueData.text) || audio != null);


        //Get new response handler
        lastPickedResponseIdx = -1;
        preselectedResponse = null;
        if (dialogueData.responses.Length > 0)
        {
            hasResponse = true;
            currResponsesDisplayer = GetResponseDisplayer(dialogueData);

            if (currResponsesDisplayer != null)
            {
                currResponsesDisplayer.ShowResponses(dialogueData.responses);

                for (int i = 0; i < grayOutResponseIdxes.Count; i++)
                {
                    currResponsesDisplayer.GrayOutResponse(grayOutResponseIdxes[i]);
                }

                if (narrativeSceneItem.shouldPreviewAnswers && !canSkipAudio.isOn)
                {
                    doneResponsePreview = false;
                    currResponsesDisplayer.SetCanInteractWithBtns(false);
                    for (int i = 0; i < dialogueData.responses.Length; i++)
                    {
                        var currResponse = dialogueData.responses[i];
                        if (currResponse.responseAudio == null) continue;
                        Debug.Log("playing " + i);
                        currResponsesDisplayer.HighlightResponse(currResponse);
                        var currPrevAudio = SelectAudioResponseByGender(currResponse);
                        audioPlayer.clip = currPrevAudio;
                        audioPlayer.Play();
                        yield return new WaitForSeconds(audioPlayer.clip.length);
                    }
                    currResponsesDisplayer.HighlightResponse(null);
                }
                doneResponsePreview = true;
                currResponsesDisplayer.SetCanInteractWithBtns(true);
            }
        }
        else
        {
            doneResponsePreview = true;
            hasResponse = false;
        }


        if (audioIsDone && !hasResponse)
        {
            continueBtn.gameObject.SetActive(true);
            skipDialogueTutImg.gameObject.SetActive(true);
        }

        currResponseTime = 0;

        while (!hasPendingLineChange)
        {
            currResponseTime += Time.deltaTime;

            if (preselectedResponse != null)
            {
                preselectedResponseAudioIsDone = !audioPlayer.isPlaying;
                if (canSkipAudio.isOn) preselectedResponseAudioIsDone = true;
                currResponsesDisplayer.ActiveConfirmationButton(preselectedResponseAudioIsDone);
            }
            yield return null;
        }


        state = dialogLineState.Exiting;
        if (dialogueData.ExitAnim != null)
        {
            timeLinePlayer.extrapolationMode = DirectorWrapMode.None;
            timeLinePlayer.playableAsset = dialogueData.ExitAnim;
            timeLinePlayer.Play();
            while (timeLinePlayer.state == PlayState.Playing) yield return null;
        }
        state = dialogLineState.NotShowing;
        hasPendingLineChange = false;
        currAnimSequence = null;

        if (!string.IsNullOrEmpty(currResponseChoiceAnalyticIDRta))
        {
            Debug.Log("cur1 " + currResponseChoiceAnalyticIDRta + " " + currResponseAnalyticResponse);
            Debug.Log("cur2 " + currResponseChoiceAnalyticIDCod + " " + currResponseAnalyticResponseVal);
            Debug.Log("cur3 " + currResponseChoiceAnalyticIDTm + " " + currResponseTime);

            narrativeSceneItem.itemAnalytics.Add(currResponseChoiceAnalyticIDRta, currResponseAnalyticResponse);
            narrativeSceneItem.itemAnalytics.Add(currResponseChoiceAnalyticIDCod, currResponseAnalyticResponseVal);
            narrativeSceneItem.itemAnalytics.Add(currResponseChoiceAnalyticIDTm, currResponseTime);


            currResponseAnalyticResponse = null;
            currResponseChoiceAnalyticIDRta = null;
            currResponseAnalyticResponseVal = -1;
            currResponseTime = 0;
        }

        if (pendingSequenceToShow != null)
        {
            ShowDialogueSequence(pendingSequenceToShow);
        }
        else NextDialogue();
    }
    public void OnClickResponse(DialogueResponse responseClicked)
    {
        if (!audioIsDone) return;

        currResponsesDisplayer.ActiveConfirmationButton(doneResponsePreview);
        //Response set for confirmation (You need to double click it to confirm)
        preselectedResponseAudioIsDone = false;
        var currResponseAudio = responseClicked.responseAudio;

        if (responseClicked.responseAudioAlternative != null)
        {
            currResponseAudio = UserDataManager.CurrUser.sexo == UserSex.Mujer ? responseClicked.responseAudioAlternative : responseClicked.responseAudio;
        }
        audioPlayer.clip = currResponseAudio;
        if (audioPlayer.clip != null) audioPlayer.Play();
        preselectedResponse = responseClicked;

    }
    public void OnClickResponseConfirmation()
    {
        if (!preselectedResponseAudioIsDone) return;
        Debug.Log("confimg");
        if (preselectedResponse.dataAfterResponse != null) pendingSequenceToShow = preselectedResponse.dataAfterResponse;

        lastPickedResponseIdx = currResponsesDisplayer.currResponses.FindIndex(x => x.ResponseData == preselectedResponse);

        var analyticInfo = CurrDialog.responses[lastPickedResponseIdx].analyticInfo;

        if (analyticInfo.mainCategory != NarrativeAnalyticCategory.NONE)
        {
            var questionIdx = analyticInfo.hasCustomID ? -1 : GetQuestionIdxFor(analyticInfo, analyticInfo.inRelationTo == NarrativeAnalticsEmpathyInRelationTo.ami ? true : false);

            currResponseChoiceAnalyticIDRta = analyticInfo.hasCustomID ? analyticInfo.rtaCustomID : analyticInfo.BuildID(
                narrativeIdx: NarrativeSceneManager.Instance.NarrativeIdx,
                questionIdx: questionIdx,
                NarrativeAnalyticType.Rta);

            currResponseChoiceAnalyticIDCod = analyticInfo.hasCustomID ? analyticInfo.codCustomID : analyticInfo.BuildID(
                narrativeIdx: NarrativeSceneManager.Instance.NarrativeIdx,
                questionIdx: questionIdx,
                NarrativeAnalyticType.cod);

            currResponseChoiceAnalyticIDTm = analyticInfo.hasCustomID ? analyticInfo.tmCustomID : analyticInfo.BuildID(
                narrativeIdx: NarrativeSceneManager.Instance.NarrativeIdx,
                questionIdx: questionIdx,
                NarrativeAnalyticType.Tm);

            currResponseAnalyticResponse = analyticInfo.BuildResponse();
            currResponseAnalyticResponseVal = analyticInfo.BuildValue();
        }

        currResponsesDisplayer.ActiveConfirmationButton(false);
        hasPendingLineChange = true;


        if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.stepConfirmedButton))
        {
            TutorialManager.Instance.TurnOffTutorialStep(tutorialSteps.stepConfirmedButton);
        }
    }

    private int EmptQuestionsCount = 0;
    private int AggQuestionsCount = -1;
    private int ConfQuestionsCount = -1;
    private int EmoCompQuestionsCount = -1;
    private int EmoBasQuestionsCount = -1;

    private int AmiQuestionsCount = -1;
    private int EneQuestionsCount = -1;


    public int GetQuestionIdxFor(NarrativeAnalyicsInfo info, bool isEmptAmi = false)
    {
        switch (info.mainCategory)
        {
            case NarrativeAnalyticCategory.Empathy:
                if (isEmptAmi)
                {
                    AmiQuestionsCount++;
                    return AmiQuestionsCount;
                }
                else
                {
                    EneQuestionsCount++;
                    return EneQuestionsCount;
                }
            case NarrativeAnalyticCategory.Aggression:
                AggQuestionsCount++;
                return AggQuestionsCount;
            case NarrativeAnalyticCategory.Conflict:
                ConfQuestionsCount++;
                return ConfQuestionsCount;
            case NarrativeAnalyticCategory.EmoBas:
                EmoBasQuestionsCount++;
                return EmoBasQuestionsCount;
            case NarrativeAnalyticCategory.EmoComp:
                EmoCompQuestionsCount++;
                return EmoCompQuestionsCount;
            default: return -1;
        }
    }

    public void AppearText()
    {
        var currDialogue = dialoguesToShow.dialogues[currShowingIdx];
        dialogueTxt.SetText(currText.ToString());
        appearTimer += Time.deltaTime;
        if (appearTimer >= appearTime)
        {
            appearTimer = 0;
            currCharProgress++;

            if (currCharProgress >= currDialogueCharacters.Length) forceEndAppearingTxt = true;
            else currText.Append(currDialogueCharacters[currCharProgress]);
        }

        if (forceEndAppearingTxt)
        {
            isAppearingTxt = false;
            dialogueTxt.SetText(SelectTextByGender(currDialogue));
            var turnOnAutoSkip = AutoContinueActive();
            fullScreenInvisibleBtn.gameObject.SetActive(turnOnAutoSkip);
        }
    }

    public void HideDialogues(bool GoToNextScene)
    {
        isAppearingTxt = false;
        isShowing = false;
        dialoguesToShow = null;
        mainDialoguesGraphics.SetActive(false);
        OnEndShowingDialogue?.Invoke();
        currShowingIdx = -1;
        if (GoToNextScene)
        {
            SaveAnalytics();
            UnloadAllImages();
            narrativeSceneItem.OnSequenceOver();
        }
    }


    public void UnloadAllImages()
    {
        var allImages = NarrativeSceneManager.Instance.allSpritesUsed;
        for (int i = 0; i < allImages.Length; i++)
        {
            if (allImages[i] == null)
            {
                continue;
            }
            if (allImages[i].texture != null)
            {
                Resources.UnloadAsset(allImages[i].texture);
            }
            Resources.UnloadAsset(allImages[i]);
        }
        var allTimeLines = NarrativeSceneManager.Instance.allAnimClips;
        for (int i = 0; i < allTimeLines.Length; i++)
        {
            if (allTimeLines[i] == null) continue;
            Resources.UnloadAsset(allTimeLines[i]);
        }
    }

    public static void SaveAnalytics()
    {
        if (instance == null) return;
        var analytics = Instance.narrativeSceneItem.itemAnalytics;
        if (analytics != null && analytics.Count > 0)
        {

            var collectionID = DataIds.Narratives1;
            if (NarrativeSceneManager.Instance.NarrativeIdx == 1) collectionID = DataIds.Narratives2;
            else if (NarrativeSceneManager.Instance.NarrativeIdx == 2) collectionID = DataIds.Narratives3;

            UserDataManager.SaveUserAnayticsPerGame(
                CollectionName: collectionID,
                itemAnalytics: Instance.narrativeSceneItem.itemAnalytics,
                shouldUseTestID: true);
        }
    }

    public List<NarrativeNavigationNode> GetCurrNavigationNodes()
    {
        //Storing the last node so that we know on which dialog line we were at the moment this history was asked for
        choicesTree.Add(new NarrativeNavigationNode(lastDisplayedDialogLineIdx));
        return choicesTree;
    }

    public string SelectTextByGender(DialogueData curr)
    {
        var text = curr.text;
        if (UserDataManager.CurrUser.sexo == UserSex.Mujer && !string.IsNullOrEmpty(curr.textAlternative))
        {
            text = curr.textAlternative;
        }

        return text;
    }

    public AudioClip SelectAudioByGender(DialogueData curr)
    {
        var audio = curr.audio;
        if (UserDataManager.CurrUser.sexo == UserSex.Mujer && curr.audioAlternative != null)
        {
            audio = curr.audioAlternative;
        }
        return audio;
    }
    public AudioClip SelectAudioResponseByGender(DialogueResponse curr)
    {
        var audio = curr.responseAudio;
        if (UserDataManager.CurrUser.sexo == UserSex.Mujer && curr.responseAudioAlternative != null)
        {
            audio = curr.responseAudioAlternative;
        }
        return audio;
    }


}

[Serializable]
[FirestoreData]
public class NarrativeNavigationNode
{
    public int sourceDialogIdx;
    public int responsePickedIdx;
    public int nextDialogCustomStartIdx;

    public NarrativeNavigationNode() 
    {
        sourceDialogIdx = -1;
        responsePickedIdx = -1;
        nextDialogCustomStartIdx = -1;
    }

    public NarrativeNavigationNode(
        int _sourceDialogIdx = 0, 
        int _responsePickedIdx = -1, 
        int _nextDialogCustomStartIdx = -1)
    {
        sourceDialogIdx = _sourceDialogIdx;
        responsePickedIdx = _responsePickedIdx;
        nextDialogCustomStartIdx = _nextDialogCustomStartIdx;
    }
}


public enum dialogLineState
{
    NotShowing,
    Entering,
    Idle,
    Exiting
}
