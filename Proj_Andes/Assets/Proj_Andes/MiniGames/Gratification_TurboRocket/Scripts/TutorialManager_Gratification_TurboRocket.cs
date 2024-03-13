using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TutorialSteps = tutorialSteps;

public class TutorialManager_Gratification_TurboRocket : MonoBehaviour, iTurboRocketManager, ITimeManagement
{
    [SerializeField] Gratification_TurboRocket_GameConfig gameConfig;
    public Gratification_TurboRocket_GameConfig levelConfig { get => gameConfig; set { } }

    [SerializeField] List<TutorialStepsTurbo> tutorialSteps = new List<TutorialStepsTurbo>();
    public TutorialStepsTurbo currTutoStep => tutorialSteps[currTutoStepIndex];
    int currTutoStepIndex;

    public bool onPlay { get; set; }

    public Transform character;
    Vector3 firstPos;
    float currentTargetSpeed;
    SphereCollider myCollider;
    Collider[] colls;
    [HideInInspector] public int starsGatheredCount { get; set; }
    public Action OnScoreChanged;
    public bool onTurbo { get; set; }
    float currentSpeed;

    public Camera cam;
    [SerializeField] Gratification_TurboRocket_CameraController camCC;
    [SerializeField] Transform finalSpin;
    public Gratification_TurboRocket_BackgroundController bk;
    public GameStages gameStages;
    public Gratification_TurboRocket_UIController ui;
    public GameRideData data;

    [SerializeField] Image turboBtn;
    [SerializeField] ParticleSystem turboParticles;
    [SerializeField] GameObject turboAnimObj;

    [SerializeField] AudioSource turboSFX;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip introAudio;
    [SerializeField] AudioClip ifSlowAudio;
    [SerializeField] AudioClip ifTurboAudio;
    [SerializeField] AudioClip letsPlayAudio;
    [SerializeField] Transform blockingPanel;

    [SerializeField] PlayableDirector endTimelineDirector;
    [SerializeField] Transform artParent;
    [SerializeField] SkinnableObject artSkinnableObj;
    public float ySpeed;
    [SerializeField] AnimationCurve ySpeedCurve;
    float yMoveProgress = 0f;
    float startYPos = 0f;

    float currentTargetAcceleration;

    Animator characterAnimator;
    IEnumerator instruction;
    public Vector3 RoadSize => bk.starsSpawner.SpawnArea.size;

    public float tutorialProgress;
    public bool stopped;

    float targetYPos;
    float playerRanXSpace;

    float iTurboRocketManager.CurrProgress => tutorialProgress;
    float iTurboRocketManager.playerCurrentSpeed { get => currentSpeed; }
    Action iTurboRocketManager.OnScoreChanges { get => OnScoreChanged; set { } }

    public Transform myTransform => transform;

    public float playerCurrentTargetSpeed => currentTargetAcceleration;

    bool iTurboRocketManager.onDoneAnim { get; set; }
    Gratification_TurboRocket_CameraController iTurboRocketManager.camCC => camCC;

    public bool endOfTuto;
    bool alreadyGivenInstruction;
    private void Awake()
    {
        iTurboRocketManager.Instance = this;
        Init();
    }
    public void Init()
    {
        artSkinnableObj.OnCurrSkinObjChanged += ReasignAnimator;
        artParent.gameObject.SetActive(true);
        character.GetComponentInChildren<ParticleSystem>().Play();

        playerRanXSpace = 0;
        targetYPos = transform.position.y;
        TryGetComponent(out myCollider);
        TryGetComponent(out ui);

    }

    private void Start()
    {
        RideBegining();
    }

    void ReasignAnimator(Transform newActiveObj)
    {
        characterAnimator = newActiveObj.GetComponentInChildren<Animator>(includeInactive: true);
    }

    public void RideBegining()
    {
        ui.StartUi();
        colls = new Collider[5];
        currentSpeed = levelConfig.regularSpeed;
        currentTargetSpeed = levelConfig.regularSpeed;
        bk.Init();
        //InitTuto();
        turboBtn.gameObject.SetActive(false);

        SetSpeedway();
        firstPos = transform.position;
        camCC.Init();
        gameStages = GameStages.Start;
        currentTargetAcceleration = gameConfig.accelerationSpeed;
    }
    public void InitTuto()
    {
        tutorialSteps[0].stepClickableObj = bk.starsSpawner.stars[0].starColl;
        tutorialSteps[1].stepClickableObj = bk.starsSpawner.stars[1].starColl;

        for (int i = 0; i < 2; i++)
        {
            tutorialSteps[i].stepDone = false;
            if(bk.starsSpawner is TutorialStarsSpawner_Gratification_TurboRocket spawner)
            {
                tutorialSteps[i].signHand = spawner.GetHand(tutorialSteps[i].stepClickableObj);
                tutorialSteps[i].signHand.transform.position = tutorialSteps[i].stepClickableObj.transform.position;
            }
        }
        for (int i = 0; i < tutorialSteps.Count; i++) tutorialSteps[i].signHand.gameObject.SetActive(false);

        ui.progressSlider.value = 0.8f;
        ui.progressSlider.gameObject.SetActive(false);
        turboBtn.gameObject.SetActive(false);
        alreadyGivenInstruction = false;
        currTutoStepIndex = 0;
        StartCoroutine(Introduction());

        currTutoStep.InitTutoStep();
    }
    IEnumerator Introduction()
    {
        yield return new WaitForSeconds(0.5f);
        TimeManager.Instance.SetNewStopTimeUser(this);
        currTutoStep.signHand.gameObject.SetActive(false);
        audioSource.clip = introAudio;
        audioSource.Play();
        yield return new WaitForSecondsRealtime(introAudio.length);
        Debug.Log("done intro");
        TimeManager.Instance.RemoveNewStopTimeUser(this);
        audioSource.clip = ifSlowAudio;
        audioSource.Play();
        yield return new WaitForSeconds(ifSlowAudio.length);
        currTutoStep.signHand.gameObject.SetActive(true);
        blockingPanel.gameObject.SetActive(false);
    }
    IEnumerator Instruction(AudioClip clip)
    {
        blockingPanel.gameObject.SetActive(true); 
        audioSource.clip = clip;
        audioSource.Play();
        yield return new WaitForSeconds(clip.length);
        blockingPanel.gameObject.SetActive(false);
    }
    void SetSpeedway()
    {
        Vector3 playerStartPos = bk.bkSize.localScale;
        playerStartPos.y = transform.position.y;
        playerStartPos.x -= (bk.bkSize.localScale.x * 1.5f);
        transform.position = playerStartPos;

        Vector3 finalSpinPos = bk.bkSize.localScale;
        finalSpinPos.y = finalSpin.transform.position.y;
        finalSpinPos.x -= bk.bkSize.localScale.x - bk.bkSize.localScale.x / 2;
        finalSpin.transform.position = finalSpinPos;
    }

    void TutoProgress()
    {
        var sliderValue = Mathf.Lerp(8, 10, playerRanXSpace / bk.bkSize.localScale.x);
        var prog = Mathf.InverseLerp(1, 10, sliderValue);
        tutorialProgress = prog;
    }
    void Update()
    {
        if (!onPlay || !iTurboRocketManager.Instance.onDoneAnim) return;
		TutoProgress();
		if (!endOfTuto)
        {
            if (transform.position.x >= currTutoStep.stepClickableObj.transform.position.x)
            {
                StandStill();
            }
            if (currTutoStep.step == TutorialStepsTurboRocket.TurboAppear && !onTurbo)
            {
                StandStill();
			}
			else if (currTutoStep.step == TutorialStepsTurboRocket.UnclickTurbo && onTurbo)
            {
                StandStill();
				turboBtn.color = Color.red;
            }
            if (playerRanXSpace / bk.bkSize.localScale.x >= 0.9) if (onTurbo && currTutoStep.step == TutorialStepsTurboRocket.TurboAppear) GoToNextStep();
        }
        ContinuousMovement();

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            startYPos = transform.position.y;
            yMoveProgress = 0;
            targetYPos = mouseWorldPos.y;
            targetYPos = Mathf.Clamp(targetYPos, -RoadSize.y / 2, RoadSize.y / 2);
        }

        var collsAmt = Physics.OverlapSphereNonAlloc(myCollider.transform.position, myCollider.radius, colls);
        for (int i = 0; i < collsAmt; i++) CollisionManagement(colls[i]);

    }


    void ContinuousMovement()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, currentTargetSpeed, currentTargetAcceleration * Time.deltaTime);
        if (onPlay)
        {
            var movementToAdd = Vector3.right * currentSpeed * Time.deltaTime;
            playerRanXSpace += movementToAdd.x;
            transform.position += movementToAdd;
            var newPos = transform.position;
            
            yMoveProgress = Mathf.Clamp01(yMoveProgress + ySpeed * Time.deltaTime);
            var yAnimProgress = ySpeedCurve.Evaluate(yMoveProgress);
            newPos.y = Mathf.Lerp(startYPos, targetYPos, yAnimProgress);
            
            transform.position = newPos;
        }
        else
        {
            transform.position = firstPos;
            onPlay = false;
        }
    }
    public void OnEnterTurboMode()
    {
        characterAnimator.SetTrigger("Turbo");
        turboParticles.Play();
        turboAnimObj.SetActive(true);
        turboSFX.Play();
        currentTargetSpeed = levelConfig.turboSpeed;
        currentTargetAcceleration = gameConfig.accelerationSpeed;
        camCC.OnEnterTurbo();
        onTurbo = true;
    }
    public void OnExitTurboMode()
    {
        characterAnimator.SetTrigger("Normal");
        camCC.OnExitTurbo();
        turboParticles.Stop();
        turboAnimObj.SetActive(false);
        turboSFX.Stop();
        currentTargetAcceleration = gameConfig.deacceleration;
        currentTargetSpeed = gameConfig.regularSpeed;
        onTurbo = false;

    }
    void CollisionManagement(Collider collider)
    {
        if (collider.CompareTag("Finish")) EndOfRide();
        else if (collider == currTutoStep.stepClickableObj) GoToNextStep();
        else if (collider.TryGetComponent(out Gratification_TurboRocket_StarsController star))
        {
            if (onTurbo) return;
            star.OnCaptured();
        }
    }

    public void GoToNextStep()
    {
        currTutoStep.EndTutoStep();
        if (currTutoStepIndex + 1 <= tutorialSteps.Count)
        {
            currTutoStepIndex++;
            currTutoStep.InitTutoStep();
            currentTargetSpeed = levelConfig.regularSpeed;
        }
        else EndOfTuto();

        if (currTutoStep.step == TutorialStepsTurboRocket.TurboAppear)
        {
            instruction = Instruction(ifTurboAudio);
            if(!alreadyGivenInstruction) StartCoroutine(instruction);
            else StopAllCoroutines();
            turboBtn.gameObject.SetActive(true);
            ui.progressSlider.gameObject.SetActive(true);
            alreadyGivenInstruction = true;
        }
    }
    void StandStill()
    {
        currentSpeed = 0;
        currentTargetSpeed = 0;
    }
    public void EndOfTuto()
    {
        endOfTuto = true;
		UserDataManager.CurrUser.RegisterTutorialStepDone(TutorialSteps.TurboRocketDone.ToString());
	}
	void EndOfRide()
    {
        bk.EndOfGame();
        starsGatheredCount += 3;
        onPlay = false;
        levelConfig.coinsCollected = starsGatheredCount;
        character.GetComponentInChildren<ParticleSystem>().Stop();
        artParent.gameObject.SetActive(false);

        StartCoroutine(_OnFinishSequence());
    }
    public IEnumerator _OnFinishSequence()
    {
        currentTargetSpeed = levelConfig.regularSpeed;
        audioSource.clip = letsPlayAudio;
        audioSource.Play();
        camCC.OnGameFinishedSequence();
        endTimelineDirector.Play();
        yield return new WaitForSeconds(letsPlayAudio.length);
        GameSequencesList.Instance.GoToNextItemInList();
    }
}

public enum TutorialStepsTurboRocket
{
    UpperStar,
    DownerStar,
    TurboAppear,
    UnclickTurbo
}

[Serializable]
public class TutorialStepsTurbo
{
    public TutorialStepsTurboRocket step;
    public Collider stepClickableObj;
    public bool stepDone;
    public TutorialHand_TurboRocket signHand;
    public Vector2 handOffset;
    public void InitTutoStep()
    {
        if(step != TutorialStepsTurboRocket.TurboAppear || step != TutorialStepsTurboRocket.UnclickTurbo) signHand.Init(stepClickableObj, handOffset);
        signHand.gameObject.SetActive(true);
    }

    public void EndTutoStep()
    {
        stepDone = true;
        signHand.gameObject.SetActive(false);
    }
}

