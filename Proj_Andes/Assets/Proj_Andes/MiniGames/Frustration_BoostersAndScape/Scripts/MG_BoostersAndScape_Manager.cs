using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class MG_BoostersAndScape_Manager : MonoBehaviour, IEndOfGameManager
{
    static MG_BoostersAndScape_Manager instance;
    public static MG_BoostersAndScape_Manager Instance => instance;
    public MG_BoostersAndScape_GameConfig gameConfig;

    public Transform rocket;
    public Transform alien;
    MG_BoostersAndScape_AlienMovementController alienMov;
    [HideInInspector] public MG_BoostersAndScape_Boosters currentBooster;

    [HideInInspector] public bool onPlay;
    int successfulAttempts;
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
    [SerializeField] GameUIController gameUIController;

    [SerializeField] EndOfGameManager eogManager;
    [SerializeField] GameObject inGameObj;

    [Header("Game Audio")]
    [SerializeField] AudioClip boosteredAudio;
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
    public EndOfGameManager EndOfGameManager => eogManager;

    //DATA ANALYTICS
    public float timePlayed;
    public int clickRepetitions;
    public int lostByCheat;
    public int boostersActivated;

    private void Awake()
    {
        if(instance != null)
        {
            if (instance != this) DestroyImmediate(this);
        }
        instance = this;
        spawner.Init();

        Init();
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
    }

    void UpdateCharacterAnimRef(Transform newCharacterArtObj)
    {
		characterAnims = newCharacterArtObj.GetComponentInChildren<Animator>(true);
	}

    void Update()
    {
        constantScoreText.text = successfulAttempts.ToString();

        if (!onPlay) return;

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
        }

        if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_BoostersAndScapeDone))
        {
            if (currentBooster.Boosteable())
            {
                Time.timeScale = 0;
                tutorialAnims.gameObject.SetActive(true);
            }
        }
        if (Input.GetMouseButtonDown(0)) clickRepetitions++;
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_BoostersAndScapeDone))
            {
                Time.timeScale = 1;
                currStepTurorial++;
                tutorialAnims.gameObject.SetActive(false);
                if (currStepTurorial == 3) UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialSteps.MG_BoostersAndScapeDone.ToString());
            }
            if (gameConfig.forceToFail) ForcedToFail();
            if (currentBooster.Boosteable())
            {
                OnBoostered(currentBooster);
            }
            else Onfailed();
        }
        if (successfulAttempts == 10)
        {
            OnGameEnd();
            Debug.Log("You won!");
            rocket.transform.position = Vector3.right * 5 * Time.deltaTime;
        }
        if (alien.transform.position.x >= rocket.transform.position.x) OnGameEnd();
    }
    void OnGameStart()
    {
        endOfGameContainer.gameObject.SetActive(false);
        inGameObj.SetActive(true);
        eogManager.OnGameStart();
        spawner.OnGameStart();
        successfulAttempts = 0;
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
        timePlayed = totalTime;
        finalScoreText.text =   successfulAttempts.ToString() ;
        endOfGameContainer.gameObject.SetActive(true);
        onPlay = false;
        spawner.OnGameEnd();
        gameConfig.SaveCoins(successfulAttempts);
        eogManager.OnGameOver();

        inGameObj.SetActive(false);
        Debug.Log("Game over!");
    }
    void Onfailed()
    {

    }
    public void OnBoostered(MG_BoostersAndScape_Boosters booster)
    {
        onBoost = true;
        targetTime = 0.3f;
        boostersActivated++;

        alienMov.OnBoosted();
        booster.Boosted();
        spawner.spawner.nextSpawnTime = targetTime;
        successfulAttempts++;
        characterAnims.SetTrigger("Turbo");
        timer = 0;
         
        
        audiosource.clip = boosteredAudio;
        audiosource.Play();
        boostedParticles.Play();
        for (int i = 0; i < skinObjAnim.Length; i++)
        {

            skinObjAnim[i].SetTrigger("Correct");

        }
    }

    public void MoveToNextPos(MG_BoostersAndScape_Boosters booster)
    {
        totalAttempts++;
        alienMov.MoveToNextPoint();
    }

    public void ForcedToFail()
    {
		for (int i = 0; i < forcedFails.Count; i++)
        {
            if (totalAttempts == forcedFails[i])
            {
                forcedFails.Remove(forcedFails[i]);
                onTrapMode = true;
				StartCoroutine(ShowTrapSign());
			}
			else onTrapMode = false;
        }
        if (successfulAttempts >= gameConfig.boostersPerRun - gameConfig.forcedFails-1)
        {
            onTrapMode = true;
            StartCoroutine(ShowTrapSign());
            lostByCheat++;
        }
    }

	IEnumerator ShowTrapSign()
	{
		trapImage.gameObject.SetActive(true);
		yield return new WaitForSeconds(1);
		trapImage.gameObject.SetActive(false);
	}

	int GenerateRandom()
    {
        return Random.Range((gameConfig.boostersPerRun - gameConfig.extraAttemptsBeforeFailing), gameConfig.boostersPerRun);
    }
}
