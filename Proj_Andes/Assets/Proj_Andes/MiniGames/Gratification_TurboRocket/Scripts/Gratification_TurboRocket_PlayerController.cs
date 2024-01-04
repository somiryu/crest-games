using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.EventSystems;
using System.Collections;

public class Gratification_TurboRocket_PlayerController : MonoBehaviour, IEndOfGameManager
{
    static Gratification_TurboRocket_PlayerController instance;
    public static Gratification_TurboRocket_PlayerController Instance => instance;

    public Gratification_TurboRocket_GameConfig levelConfig;
    public bool onPlay;
    public Transform character;
    Vector3 firstPos;
    float currentTargetSpeed;
    SphereCollider myCollider;
    Collider[] colls;
    [HideInInspector] public int starsGatheredCount;
    public bool onTurbo = false;
    float currentSpeed;
    public Camera cam;
    [SerializeField] Gratification_TurboRocket_CameraController camCC;
    [SerializeField] Transform finalSpin;
    public Gratification_TurboRocket_BackgroundController bk;
    public GameStages gameStages;
    public Gratification_TurboRocket_UIController ui;
    public GameRideData data;
    [SerializeField] EndOfGameManager eogManager;

    [SerializeField] ParticleSystem turboParticles;
    [SerializeField] AudioSource turboSFX;
    [SerializeField] PlayableDirector endTimelineDirector;
    [SerializeField] Transform artParent;
    [SerializeField] SkinnableObject artSkinnableObj;

    Animator characterAnimator;

    public EndOfGameManager EndOfGameManager => eogManager;
    public Vector3 RoadSize => bk.starsSpawner.SpawnArea.size;


    Ray hit;
	float timer;
	float turboTimer;
    float targetYPos;
    float playerRanXSpace;

    public float CurrProgress
    {
        get
        {
            return playerRanXSpace / bk.bkSize.localScale.x;
        }
    }

    public Vector3 CurrPos
    {
        get
        {
            return transform.position;
        }
    }

    private void Awake()
    {
        if (instance != null)
        {
            if(instance != this)
            {
                DestroyImmediate(this);
            }
        }
        instance = this;
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
        //camCC = GetComponentInChildren<Gratification_TurboRocket_CameraController>();
        eogManager.OnGameStart();
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
		SetSpeedway();
		firstPos = transform.position;
        onPlay = true;
        gameStages = GameStages.Start;
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
        if (!onPlay) return;
        ContinuousMovement();
        if (Input.GetMouseButtonDown(0) &&!EventSystem.current.IsPointerOverGameObject())
        {
            var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            targetYPos = mouseWorldPos.y;
            targetYPos = Mathf.Clamp( targetYPos, -RoadSize.y / 2, RoadSize.y / 2);
        }

        var collsAmt = Physics.OverlapSphereNonAlloc(myCollider.transform.position, myCollider.radius, colls);
        for (int i = 0; i < collsAmt; i++) CollisionManagement(colls[i]);

    }
    void ContinuousMovement()
    {
        currentSpeed = Mathf.MoveTowards(currentSpeed, currentTargetSpeed, levelConfig.accelerationSpeed * Time.deltaTime);
        timer += Time.deltaTime;
        if (timer <= levelConfig.regularRideDuration)
        {
            var movementToAdd = Vector3.right * currentSpeed * Time.deltaTime;
            playerRanXSpace += movementToAdd.x;
            transform.position += movementToAdd;
            var currPos = transform.position;
            currPos.y = Mathf.MoveTowards(currPos.y, targetYPos, currentSpeed * Time.deltaTime);
            transform.position = currPos;
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
        turboSFX.Stop();


        onTurbo = false;
    }
    void CollisionManagement(Collider collider)
    {
        if (collider.CompareTag("Finish")) EndOfRide();
        else if (collider.TryGetComponent(out Gratification_TurboRocket_StarsController star))
        {
            if (onTurbo) return;
            star.OnCaptured();
        }
    }
 
    void EndOfRide()
    {
        var ride = new GameRideData();
        ride.starsCollected = starsGatheredCount;
        ride.turboSelectedTime = turboTimer;
        ride.totalRideDuration = timer;
        ride.totalStars = levelConfig.starsAmount;
        data = ride;
        bk.EndOfGame();
        onPlay = false;
		character.GetComponentInChildren<ParticleSystem>().Stop();
		artParent.gameObject.SetActive(false);

        StartCoroutine(_OnFinishSequence());
    }
    IEnumerator _OnFinishSequence()
    {
        camCC.OnGameFinishedSequence();
        endTimelineDirector.Play();
        yield return new WaitForSeconds(2f);
        ui.EndOfGame();
        eogManager.OnGameOver();
        gameStages = GameStages.End;
    }
}


public class GameRideData
{
    public float turboSelectedTime;
    public int starsCollected;
    public int totalStars;
    public float totalRideDuration;
}

public enum GameStages
{
    Start,
    End
}