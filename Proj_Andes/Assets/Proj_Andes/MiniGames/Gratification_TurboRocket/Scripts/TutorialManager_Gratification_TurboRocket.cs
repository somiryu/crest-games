using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class TutorialManager_Gratification_TurboRocket : MonoBehaviour, iTurboRocketManager
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
    [SerializeField] PlayableDirector endTimelineDirector;
    [SerializeField] Transform artParent;
    [SerializeField] SkinnableObject artSkinnableObj;
    public float ySpeed;
    [SerializeField] AnimationCurve ySpeedCurve;
    float yMoveProgress = 0f;
    float startYPos = 0f;



    Animator characterAnimator;

    public Vector3 RoadSize => bk.starsSpawner.SpawnArea.size;


    Ray hit;
    float timer;
    float turboTimer;
    float targetYPos;
    float playerRanXSpace;

    float iTurboRocketManager.CurrProgress { get => playerRanXSpace / bk.bkSize.localScale.x; }
    float iTurboRocketManager.playerCurrentSpeed { get => currentSpeed; }
    Action iTurboRocketManager.OnScoreChanges { get => OnScoreChanged; set { } }

    public Transform myTransform => transform;
    public bool endOfTuto;
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

        InitTuto();


        SetSpeedway();
        firstPos = transform.position;
        onPlay = true;
        gameStages = GameStages.Start;
    }
    void InitTuto()
    {
        tutorialSteps[0].stepClickableObj = bk.starsSpawner.stars[0].starColl;
        tutorialSteps[1].stepClickableObj = bk.starsSpawner.stars[1].starColl;
        tutorialSteps[2].stepClickableObj = bk.starsSpawner.stars[2].starColl;

        for (int i = 0; i < tutorialSteps.Count; i++)
        {
            tutorialSteps[i].stepDone = false;
            if(bk.starsSpawner is TutorialStarsSpawner_Gratification_TurboRocket spawner) tutorialSteps[i].signHand = spawner.GetHand(tutorialSteps[i].stepClickableObj);
            tutorialSteps[i].signHand.gameObject.SetActive(false);
        }

        ui.progressSlider.gameObject.SetActive(false);
        turboBtn.gameObject.SetActive(false);

        currTutoStepIndex = 0;
        currTutoStep.InitTutoStep();
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

    void Update()
    {
        if (!onPlay) return;

        if (!endOfTuto)
        {
            if (transform.position.x >= currTutoStep.stepClickableObj.transform.position.x) StandStill();
            if (playerRanXSpace / bk.bkSize.localScale.x >= 0.9)
            {
                if (onTurbo)
                {
                    StandStill();
                }
            }
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
        currentSpeed = Mathf.MoveTowards(currentSpeed, currentTargetSpeed, levelConfig.accelerationSpeed * Time.deltaTime);
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
            timer = 0;
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
        camCC.OnEnterTurbo();
        onTurbo = true;
    }
    public void OnExitTurboMode()
    {
        characterAnimator.SetTrigger("Normal");
        currentTargetSpeed = levelConfig.regularSpeed;
        camCC.OnExitTurbo();
        turboParticles.Stop();
        turboAnimObj.SetActive(false);
        turboSFX.Stop();


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
        currTutoStepIndex++;
        currTutoStep.InitTutoStep();
        currentTargetSpeed = levelConfig.regularSpeed;

        if(currTutoStep.step == TutorialStepsTurboRocket.TurboAppear)
        {
            currentSpeed = 0;
            turboBtn.color = Color.red;
            turboBtn.gameObject.SetActive(true);
            ui.progressSlider.gameObject.SetActive(true);
        }
    }
    void StandStill()
    {
        currentSpeed = 0;
    }
    public void Continue()
    {
        currentTargetSpeed = levelConfig.regularSpeed;
    }
    void EndOfRide()
    {
        bk.EndOfGame();
        onPlay = false;
        levelConfig.coinsCollected = starsGatheredCount;
        character.GetComponentInChildren<ParticleSystem>().Stop();
        artParent.gameObject.SetActive(false);

        StartCoroutine(_OnFinishSequence());
    }
    public IEnumerator _OnFinishSequence()
    {
        camCC.OnGameFinishedSequence();
        endTimelineDirector.Play();
        yield return new WaitForSeconds(2f);
        ui.EndOfGame();
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

    public void InitTutoStep()
    {
        Debug.Log("starting " + step.ToString());
        signHand.Init(stepClickableObj);
        signHand.gameObject.SetActive(true);
    }

    public void EndTutoStep()
    {
        stepDone = true;
        signHand.gameObject.SetActive(false);
    }
}

