using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

public class MG_BoostersAndScape_Manager : MonoBehaviour, IEndOfGameManager, ITimeManagement
{
    static MG_BoostersAndScape_Manager instance;
    public static MG_BoostersAndScape_Manager Instance => instance;
    public MG_BoostersAndScape_GameConfig gameConfig;

    public Transform rocket;
    public Transform alien;
    MG_BoostersAndScape_AlienMovementController alienMov;
    [HideInInspector] public MG_BoostersAndScape_Boosters currentBooster;

    [HideInInspector] public bool onPlay;
    int successfulAttemptsScore;
    int totalAttempts;
    [HideInInspector] public float catchBoosterRange;
    [HideInInspector] public bool onTrapMode;
    [HideInInspector] public float timer;
    bool onBoost;
    float targetTime;
    Vector3 startPos;
    float totalTime;

    public List<int> forcedFails = new List<int>();
    [SerializeField] MG_BoostersAndScape_Spawner spawner;
    [SerializeField] Transform endOfGameContainer;
    [SerializeField] TextMeshProUGUI finalScoreText;
    [SerializeField] TextMeshProUGUI constantScoreText;
    [SerializeField] Image trapImage;
    [SerializeField] GameUIController gameUIController => GameUIController.Instance;

    [SerializeField] EndOfGameManager eogManager;
    [SerializeField] GameObject inGameObj;

    [Header("Game Audio")]
    [SerializeField] AudioClip boosteredAudio;
    [SerializeField] AudioClip onFailedAudio;
    [SerializeField] AudioClip introAudio;
    [SerializeField] AudioClip letsPlay;
    [SerializeField] AudioClip noStartsAudio;
    [SerializeField] AudioSource audioInstructions;
    [SerializeField] Transform blockingPanel;
    private AudioSource audiosource;


    [Header("GameParticles")]
    [SerializeField] ParticleSystem boostedParticles;
    [SerializeField] ParticleSystem incorrectParticles;
    [SerializeField] GameObject skinObj;
    [SerializeField] Animator[] skinObjAnim;

    [Header("Player Anims")]
    [SerializeField] SkinnableObject charactetSkinableObj;
    [SerializeField] Animator characterAnims;

    [Header("Tutorial Anims")]
    [SerializeField] Animator tutorialAnims;
    int currStepTurorial;
    public BoostersAndScapeAnalytics currAnalytic;

    public List<BoostersAndScapeAnalytics> currAnalytics = new List<BoostersAndScapeAnalytics>();
    public EndOfGameManager EndOfGameManager => eogManager;
    [SerializeField] CatchCoinsAudioInstruction audioInstruction;

    //DATA ANALYTICS
    public float timePlayed;
    public int clickRepetitions;
    public int lostByCheat;
    public int boostersActivated;
    bool addedTimeManagUser;

    int trials = 0;
    private void Awake()
    {
        if(instance != null)
        {
            if (instance != this) DestroyImmediate(this);
        }
        instance = this;
        spawner.Init();
        if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_BoostersAndScapeDone)) GameUIController.Instance.onTuto = true;

        audiosource = GetComponent<AudioSource>();
    }
    void Init()
    {
        currStepTurorial = 0;
        skinObjAnim = skinObj.GetComponentsInChildren<Animator>(true);
        charactetSkinableObj.OnCurrSkinObjChanged += UpdateCharacterAnimRef;
        tutorialAnims.gameObject.SetActive(false);

        totalTime = 0;

        alien.TryGetComponent(out alienMov);
        alienMov.Init();
        startPos = rocket.transform.position;
        startPos.x = 0;
        rocket.transform.position = startPos;
        targetTime = gameConfig.boosterTriggerRate;
        endOfGameContainer.gameObject.SetActive(false);
        catchBoosterRange = 1.5f;
        OnGameStart();
        trapImage.gameObject.SetActive(false);
        var newRound = new BoostersAndScapeAnalytics();
        currAnalytic = newRound;
        if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_BoostersAndScapeDone)) StartCoroutine(Intro());  
    }
    IEnumerator Intro()
    {
        TimeManager.Instance.SetNewStopTimeUser(this);
        blockingPanel.gameObject.SetActive(true);
        audiosource.clip = introAudio;
        audiosource.Play();
        yield return new WaitForSecondsRealtime(introAudio.length);
        blockingPanel.gameObject.SetActive(false);
        TimeManager.Instance.RemoveNewStopTimeUser(this);
    }
    private void Start()
	{
        Init();
        GeneralGameAnalyticsManager.Instance.Init(DataIds.boostersAndScapeGame);
		spawner.spawner.SpawnNewItem();
	}
    public void RegisterDistanceForAnalytic(Vector3 boosterPos)
    {
        currAnalytic.distanceInBetween = (boosterPos - rocket.position).magnitude;
    }
    void UpdateCharacterAnimRef(Transform newCharacterArtObj)
    {
		characterAnims = newCharacterArtObj.GetComponentInChildren<Animator>(true);
	}

    void Update()
    {
        constantScoreText.text = successfulAttemptsScore.ToString();

        if (!onPlay) return;
        if (Input.GetMouseButtonDown(0)) currAnalytic.clicksToBoost++; 
        totalTime += Time.deltaTime;
        timer += Time.deltaTime;
        spawner.spawner.timer = timer;
        if(timer >= targetTime)
        {
            if (onBoost)
            {
                onBoost = false;
                targetTime = gameConfig.boosterTriggerRate;
            }
            timer = 0;
            MoveToNextPos(currentBooster);
            spawner.spawner.SpawnNewItem();
        }

        if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_BoostersAndScapeDone))
        {
            if (currentBooster.Boosteable())
            {
                if(!addedTimeManagUser)
                {
                    TimeManager.Instance.SetNewStopTimeUser(this);
                    Debug.Log("adding");
                    addedTimeManagUser = true;
                }
                tutorialAnims.gameObject.SetActive(true);
            }
        }
        if (Input.GetMouseButtonDown(0)) clickRepetitions++;
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_BoostersAndScapeDone))
            {
                if(addedTimeManagUser)
                {
                    TimeManager.Instance.RemoveNewStopTimeUser(this);
                    Debug.Log("removing");
                    addedTimeManagUser = false;
                }
                currStepTurorial++;
                tutorialAnims.gameObject.SetActive(false);
                if (currStepTurorial == 3)
                {
                    StartCoroutine(LetsPlay());
                    UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialSteps.MG_BoostersAndScapeDone.ToString());
                }
            }

            if (!TryForcedToFail())
            {
				if (currentBooster.Boosteable())
				{
					OnBoostered(currentBooster);
				}
				else Onfailed();
			}
			else Onfailed();
		}
        if (successfulAttemptsScore == gameConfig.boostersPerRun)
        {
            OnGameEnd();
            rocket.transform.position = Vector3.right * 5 * Time.deltaTime;
        }
        if (alien.transform.position.x >= rocket.transform.position.x) OnGameEnd();
    }
    public void StoreAnalytics(MG_BoostersAndScape_Boosters booster)
    { 
        trials++;
        currAnalytic.distanceInBetween = booster.DistanceInBoost;
        currAnalytic.roundCount = trials;
        currAnalytic.clicksToBoost = clickRepetitions;
        currAnalytics.Add(currAnalytic);
        Debug.Log("new one " + trials);

        clickRepetitions = 0;
        var newRound = new BoostersAndScapeAnalytics();
        currAnalytic = newRound;
    }
    void OnGameStart()
    {
        endOfGameContainer.gameObject.SetActive(false);
        inGameObj.SetActive(true);
        eogManager.OnGameStart();
        spawner.OnGameStart();
        successfulAttemptsScore = 0;
        alienMov.OnGameStart();
        rocket.transform.position = startPos;
        totalAttempts = 0;
        for (int i = 0; i < gameConfig.forcedFails; i++)
        {
            forcedFails.Add(GenerateRandom());
        }
        onPlay = true;

    }
    void OnGameEnd()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        if (successfulAttemptsScore > 0)
        {
            successfulAttemptsScore = 0;
            Onfailed();
        }
        blockingPanel.gameObject.SetActive(true);
		timePlayed = totalTime;
		finalScoreText.text = successfulAttemptsScore.ToString();
        inGameObj.SetActive(false);
        endOfGameContainer.gameObject.SetActive(true);
		onPlay = false;
		spawner.OnGameEnd();
		gameConfig.SaveCoins(successfulAttemptsScore);
		eogManager.OnGameOver();
		audiosource.clip = noStartsAudio;
		audiosource.Play();
        yield return new WaitForSeconds(noStartsAudio.length);
		blockingPanel.gameObject.SetActive(false);
		Debug.Log("Game over!");
	}

    public void Onfailed()
    {
        Debug.Log("Failed boost");
        audiosource.clip = onFailedAudio;
        audiosource.Play();
        successfulAttemptsScore -= gameConfig.coinsOnFailure;
        if (successfulAttemptsScore < 0) successfulAttemptsScore = 0;
        GeneralGameAnalyticsManager.RegisterLose();
        gameUIController.StarLost();
    }
    public void OnBoostered(MG_BoostersAndScape_Boosters booster)
    {
        onBoost = true;
        targetTime = 0.3f;
        boostersActivated++;
        GeneralGameAnalyticsManager.RegisterWin();
        Debug.Log("Boosted");

        alienMov.OnBoosted();
        booster.Boosted();
        spawner.spawner.nextSpawnTime = targetTime;
        successfulAttemptsScore++;
        characterAnims.SetTrigger("Turbo");
        timer = 0;

        gameUIController.StarEarned(Camera.main.WorldToScreenPoint(rocket.transform.position));

        audiosource.clip = boosteredAudio;
        audiosource.Play();
        boostedParticles.Play();
        for (int i = 0; i < skinObjAnim.Length; i++)
        {

            skinObjAnim[i].SetTrigger("Correct");

        }
    }
    IEnumerator LetsPlay()
    {
        audioInstructions.clip = letsPlay;
        audioInstructions.Play();
        yield return new WaitForSecondsRealtime(letsPlay.length);
    }
    public void MoveToNextPos(MG_BoostersAndScape_Boosters booster)
    {
        StoreAnalytics(currentBooster);
        totalAttempts++;
        alienMov.MoveToNextPoint();
    }

    public bool TryForcedToFail()
    {
        if (!gameConfig.forceToFail) return false;
		for (int i = 0; i < forcedFails.Count; i++)
        {
            if (totalAttempts == forcedFails[i])
            {
                forcedFails.Remove(forcedFails[i]);
                onTrapMode = true;
                StartCoroutine(ShowTrapSign());
                GeneralGameAnalyticsManager.RegisterLose();
                lostByCheat++;
                break;
            }
            else onTrapMode = false;
        }
        return onTrapMode;
    }

	IEnumerator ShowTrapSign()
	{
		trapImage.gameObject.SetActive(true);
		yield return new WaitForSeconds(1);
		trapImage.gameObject.SetActive(false);
	}

	int GenerateRandom()
    {
        var newRandomFailIdx = Random.Range(gameConfig.extraAttemptsBeforeFailing, gameConfig.boostersPerRun);
        if (forcedFails.Contains(newRandomFailIdx)) return GenerateRandom();
        return newRandomFailIdx;
    }
}

public class BoostersAndScapeAnalytics
{
    public int roundCount;
    public float distanceInBetween;
    public int clicksToBoost;
}

