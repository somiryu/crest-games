using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using Transform = UnityEngine.Transform;

public class Gratification_TurboRocket_PlayerController : MonoBehaviour, IEndOfGameManager, iTurboRocketManager
{
    [SerializeField] Gratification_TurboRocket_GameConfig gameConfig;
    public Gratification_TurboRocket_GameConfig levelConfig { get => gameConfig; set { } }
    public bool onPlay { get; set; }
    [SerializeField] MG_SizeRockets_Planet planet;
    public Transform character;
    Vector3 firstPos;
    public float currentTargetSpeed;
    float currentTargetAcceleration;
    SphereCollider myCollider;
    Collider[] colls;
    [HideInInspector] public int starsGatheredCount { get; set; }
    public Action OnScoreChanges { get; set; }
    public bool onTurbo { get; set; }
    float currentSpeed;
    public Camera cam;
    [SerializeField] Gratification_TurboRocket_CameraController camCC;
    [SerializeField] Transform finalSpin;
    public Gratification_TurboRocket_BackgroundController bk;
    public GameStages gameStages;
    public Gratification_TurboRocket_UIController ui;
    [SerializeField] EndOfGameManager eogManager;

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
    IEnumerator turboDeacceleration;
    float turboTimer;
    float totalTurboTimer;

	[SerializeField] Animator characterAnimator;

    public EndOfGameManager EndOfGameManager => eogManager;
    public Vector3 RoadSize => bk.starsSpawner.SpawnArea.size;


    Ray hit;
	float timer;
    float targetYPos;
    float playerRanXSpace;

    public float CurrProgress { get => (playerRanXSpace / (finalSpin.transform.position -firstPos).magnitude); }
    public Vector3 CurrPos => transform.position;
    float iTurboRocketManager.playerCurrentSpeed { get => currentSpeed; }
    public Transform myTransform { get => transform; }

    public float playerCurrentTargetSpeed => currentTargetAcceleration;


    bool iTurboRocketManager.onDoneAnim { get; set; }
    public float gameDuration { get => gameConfig.regularRideDuration; set { } }

    public float starsAmt { get => gameConfig.starsAmount; set { } }

    public bool onAnim;

    private void Awake()
    {
        iTurboRocketManager.Instance = this;
		artSkinnableObj.OnCurrSkinObjChanged += ReasignAnimator;
		TryGetComponent(out myCollider);
		TryGetComponent(out ui);
		playerRanXSpace = 0;
		targetYPos = transform.position.y;
	}

	private void Start()
	{
		artParent.gameObject.SetActive(true);
		character.GetComponentInChildren<ParticleSystem>().Play();
		planet.Init(5);
		eogManager.OnGameStart();
		turboDeacceleration = TurboCounter();
		RideBegining();
        GeneralGameAnalyticsManager.Instance.Init(DataIds.turboRocketGame);
	}

	void ReasignAnimator(Transform newActiveObj)
    {
        characterAnimator = newActiveObj.GetComponentInChildren<Animator>(includeInactive: true);
    }

	public void RideBegining()
	{
		ui.StartUi();
        ui.StartCoroutine(ui.CameraMovement(2));
		colls = new Collider[5];
		currentSpeed = levelConfig.regularSpeed;
		currentTargetSpeed = levelConfig.regularSpeed;
		bk.Init();
		SetSpeedway();
        camCC.Init();
		firstPos = transform.position;
        onPlay = true;
        gameStages = GameStages.Start;
        currentTargetAcceleration = gameConfig.accelerationSpeed;
        totalTurboTimer = 0;
    }

	void SetSpeedway()
    {
        Vector3 playerStartPos = bk.bkSize.localScale;
        playerStartPos.y = transform.position.y;
        playerStartPos.x -= (bk.bkSize.localScale.x * 1.5f);
        transform.position = playerStartPos;

        Vector3 finalSpinPos = bk.bkSize.localScale;
        finalSpinPos.y = finalSpin.transform.position.y;
        finalSpinPos.x -= bk.bkSize.localScale.x - bk.bkSize.localScale.x/2;
        finalSpin.transform.position = finalSpinPos;
    }

    void Update()
    {
        if (!onPlay || !iTurboRocketManager.Instance.onDoneAnim) return;

        ContinuousMovement();
        if (Input.GetMouseButtonDown(0) &&!EventSystem.current.IsPointerOverGameObject())
        {
            var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            startYPos = transform.position.y;
            yMoveProgress = 0;
			targetYPos = mouseWorldPos.y;
            targetYPos = Mathf.Clamp( targetYPos, -RoadSize.y / 2, RoadSize.y / 2);
        }

        var collsAmt = Physics.OverlapSphereNonAlloc(myCollider.transform.position, myCollider.radius, colls);
        for (int i = 0; i < collsAmt; i++) CollisionManagement(colls[i]);
        if (onTurbo) totalTurboTimer += Time.deltaTime;
    }


    void ContinuousMovement()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, currentTargetSpeed, currentTargetAcceleration * Time.deltaTime);
        timer += Time.deltaTime;
        if (timer <= levelConfig.regularRideDuration)
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
        if (onTurbo) turboTimer += Time.deltaTime;
    }
    public IEnumerator TurboCounter()
    {
        while (turboTimer <= levelConfig.minTurboTime)
        {
            yield return null;
        }

        onTurbo = false;
        characterAnimator.SetTrigger("Normal");
        camCC.OnExitTurbo();
        turboParticles.Stop();
        turboAnimObj.SetActive(false);
        turboSFX.Stop();
        currentTargetAcceleration = gameConfig.accelerationSpeed;
        currentTargetSpeed = gameConfig.regularSpeed;

        turboDeacceleration = null;
    }

    public void OnEnterTurboMode()
    {
        GeneralGameAnalyticsManager.RegisterLose();
		characterAnimator.ResetTrigger("Normal");
        characterAnimator.SetTrigger("Turbo");
        turboParticles.Play();
        turboAnimObj.SetActive(true);
        turboSFX.Play();
        currentTargetSpeed = levelConfig.turboSpeed;
        currentTargetAcceleration = levelConfig.accelerationSpeed;
        camCC.OnEnterTurbo();
        turboTimer = 0;
        onTurbo = true;
        gameConfig.turboUsedTimes++;

    }
    public void OnExitTurboMode()
    {
        characterAnimator.ResetTrigger("Turbo");
        if(turboDeacceleration != null) StopCoroutine(turboDeacceleration);
		turboDeacceleration = TurboCounter();
		StartCoroutine(turboDeacceleration);
    }


    void CollisionManagement(Collider collider)
    {
        if (collider.CompareTag("Finish")) EndOfRide();
        else if (collider.TryGetComponent(out Gratification_TurboRocket_StarsController star))
        {
            if (onTurbo) return;
            if (star.isCaptured) return;
            if (!star.isInitialPosition) return;
            star.OnCaptured();
            GeneralGameAnalyticsManager.RegisterWin();
            gameConfig.coinsCollected++;
        }
    }
 
    void EndOfRide()
    {
        starsGatheredCount += 5;
        OnScoreChanges?.Invoke();
        planet.UpdateCoinsAmount(0);
        bk.EndOfGame();
        Debug.Log(totalTurboTimer);
        gameConfig.totalTurboTime = totalTurboTimer;

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
        ui.progressSlider.value = 1;
        yield return new WaitForSeconds(2f);
        ui.EndOfGame();
        eogManager.OnGameOver();
        gameStages = GameStages.End;
    }
}


public enum GameStages
{
    Start,
    End
}

public interface iTurboRocketManager
{
    public static iTurboRocketManager Instance { get; set; }
    public Gratification_TurboRocket_GameConfig levelConfig { get; set; }
    public Transform myTransform { get; }
    public int starsGatheredCount { get; set; }
    public float CurrProgress { get; }
    Vector3 CurrPos => myTransform.position;
    public float playerCurrentSpeed { get; }
    public float playerCurrentTargetSpeed { get; }
    public bool onPlay { get; set; }
    public bool onDoneAnim { get; set; }
    public bool onTurbo { get; set; }
    public Gratification_TurboRocket_CameraController camCC => camCC;   
    public Action OnScoreChanges { get; set; }
    public void RideBegining();
    public void OnEnterTurboMode();
    public void OnExitTurboMode();
    public IEnumerator _OnFinishSequence();
    public float gameDuration { get; set; }
    public float starsAmt { get; set; }
}