using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MG_MagnetsGameManager : MonoBehaviour, IEndOfGameManager, ITimeManagement
{
	private static MG_MagnetsGameManager instance;
	public static MG_MagnetsGameManager Instance => instance;

    public Pool<MG_MagnetsEnergyItem> energyItemsPool;

	[SerializeField] MG_MagnetsConfigs gameConfigs;
	[SerializeField] BoxCollider spawnArea;
	[SerializeField] MG_MagnetRangeHandler magnetRangeIndicator;
	[SerializeField] AudioClip lostGameAudio;
	[SerializeField] AudioClip failEveryEnergyItemAudio;

	[Header("Game UI")]
	[SerializeField] Image EnergyFillImage;
	[SerializeField] List<Image> magnetsAvailable;
	[SerializeField] Image trapImage;
	[SerializeField] TMP_Text inGame_currPointsTextUI;
	[SerializeField] Animator noLeftMagnetsAnims;
	GameUIController gameUi => GameUIController.Instance;

	[Header("after action UI")]
	[SerializeField] GameObject afterActionPanel;
	[SerializeField] GameObject ingameObj;
	[SerializeField] GameObject ingameObjUI;
	[SerializeField] Image afterActionEnergyFillImage;
	[SerializeField] GameObject winTitle;
	[SerializeField] GameObject loseTitle;
	[SerializeField] TMP_Text afterAction_currPointsTextUI;
	[SerializeField] AudioSource instructionSource;
	[SerializeField] AudioClip capturedItemSfx;
	[SerializeField] AudioClip introAudio;
	[SerializeField] AudioClip letsPlayAudio;
	[SerializeField] AudioClip noStarsAudio;
	[SerializeField] Transform blockingPanel;
	[SerializeField] CatchCoinsAudioInstruction audioInstruction;


	public int currSpawnedItems;
	private float timer;
	private int availableMagnets;
	private int currEnergyPicked;
	private float currEneryProgress;
    private AudioSource audiosource;


    private Collider[] overlayResults = new Collider[20];
	[SerializeField] EndOfGameManager eogManager;
	int attempts;
	float totalTime;
	public EndOfGameManager EndOfGameManager => eogManager;

	//DATA ANALYTICS
	public float timePlayed;
	public int clickRepetitions;
	public int lostByCheat;
	public int magnetsCollected;
	IEnumerator onCoinLost;
    public void Awake()
	{
		if(instance != null && instance != this) DestroyImmediate(instance);
		instance = this;
		if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_Magnets_1NoClick)) GameUIController.Instance.onTuto = true;
	}

	public void Init()
    {
        audiosource = GetComponent<AudioSource>();

        ingameObj.SetActive(true);
		ingameObjUI.SetActive(true);
		clickRepetitions = 0;
		totalTime = 0;

		attempts = 0;
        energyItemsPool.Init(30);
		availableMagnets = gameConfigs.initialMagnetsCount;
		timer = gameConfigs.timeBetweenSpawnsPerDifficultLevel.GetValueModify();
		currSpawnedItems = 0;
		currEnergyPicked = 0;
		currEneryProgress = 0;
		EnergyFillImage.fillAmount = 0;
		for (int i = 0; i < magnetsAvailable.Count; i++)
		{
			magnetsAvailable[i].gameObject.SetActive(true);
		}
		magnetRangeIndicator.Init(gameConfigs.userMagnetRadius);
		trapImage.gameObject.SetActive(false);
		eogManager.OnGameStart();
		if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_Magnets_1NoClick)) StartCoroutine(Intro());

    }
    IEnumerator Intro()
	{
		blockingPanel.gameObject.SetActive(true);
		TimeManager.Instance.SetNewStopTimeUser(this);
        StartCoroutine(audioInstruction.FirstInstruction());
        yield return new WaitForSecondsRealtime(audioInstruction.firstInstruction.length);
        audiosource.clip = introAudio;
		audiosource.Play();
		yield return new WaitForSecondsRealtime(introAudio.length);
        TimeManager.Instance.RemoveNewStopTimeUser(this);
        blockingPanel.gameObject.SetActive(false);
    }
    private void Start()
	{
		Init();
		GeneralGameAnalyticsManager.Instance.Init(DataIds.magnetsGame);
	}

	private void Update()
	{
		//if (CatchCoinsAudioInstruction.Instance.firstAudio != null) return;
		if (availableMagnets == 0) return;
		timer += Time.deltaTime;
		totalTime += Time.deltaTime;
		if (Input.GetMouseButtonDown(0)) clickRepetitions++;
        if (timer > gameConfigs.timeBetweenSpawnsPerDifficultLevel.GetValueModify())
		{
			timer = 0;
			for (int i = 0; i < gameConfigs.itemsAmountToSpawn; i++)
			{
				if (currSpawnedItems >= 4 && !UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_Magnets_2FourItemEnergyClick))
				{
					if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_Magnets_1NoClick))
					{
						TutorialManager.Instance.TurnOffTutorialStep(tutorialSteps.MG_Magnets_1NoClick);
					}
					return;
				}
                SpawnNewItem();
            }
        }

        
        if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_Magnets_1NoClick)) return;
				

		if (Input.GetMouseButtonDown(0))
		{			
            var mouseGlobalPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			mouseGlobalPosition.z = 0;
			attempts++;
			if(gameConfigs.activeCheats && attempts > gameConfigs.startFaillingAfterAttemps)
			{
				mouseGlobalPosition = GetBadMousePosition(0);
				lostByCheat++;
				currEnergyPicked -= gameConfigs.coinsOnFailure;
				currEnergyPicked = Mathf.Max(currEnergyPicked, 0);
                inGame_currPointsTextUI.text = currEnergyPicked.ToString();
				gameUi.StarLost();
				StartCoroutine(ShowTrapSign());
			}
			magnetRangeIndicator.ShowAt(mouseGlobalPosition);
			
			var hitAmount = Physics.OverlapSphereNonAlloc(mouseGlobalPosition, gameConfigs.userMagnetRadius, overlayResults);
			var hitEnergyItem = 0;          

            for (int i = 0; i < hitAmount; i++)
			{
				var curr = overlayResults[i];
				if (!curr.TryGetComponent(out MG_MagnetsEnergyItem energyItem)) continue;
                if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_Magnets_2FourItemEnergyClick))
                {
					attempts = 0;
					GameUIController.Instance.onTuto = false;
					StartCoroutine(LetsPlay());
                    UserDataManager.CurrUser.RegisterTutorialStepDone(tutorialSteps.MG_Magnets_2FourItemEnergyClick.ToString());
                    TutorialManager.Instance.TurnOffTutorialStep(tutorialSteps.MG_Magnets_2FourItemEnergyClick);
                }
				hitEnergyItem++;
                OnPicketEnergyItem(energyItem);
				
			}

			if(hitEnergyItem == 0)
			{
                audiosource.clip = failEveryEnergyItemAudio;
                audiosource.Play();
				GeneralGameAnalyticsManager.RegisterLose();
			}

			if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_Magnets_2FourItemEnergyClick)) return;
                
			availableMagnets--;
			for (int i = 0; i < magnetsAvailable.Count; i++)
			{
				magnetsAvailable[i].gameObject.SetActive(i < availableMagnets);
			}
			if(availableMagnets == 0)
			{
                OnGameOver();
            }
        }
	}
	IEnumerator LetsPlay()
	{
		instructionSource.clip = letsPlayAudio;
		instructionSource.Play();
		yield return new WaitForSeconds(letsPlayAudio.length);
	}
	bool PredictIfWouldWin(Vector3 posToTest)
	{
		var hitAmount = Physics.OverlapSphereNonAlloc(posToTest, gameConfigs.userMagnetRadius, overlayResults);
		return currEnergyPicked + hitAmount >= gameConfigs.neededEnergyToPick;
	}

	IEnumerator ShowTrapSign()
	{
		trapImage.gameObject.SetActive(true);
		yield return new WaitForSeconds(1);
		trapImage.gameObject.SetActive(false);
	}

	Vector2 GetBadMousePosition(int currTrialIdx)
	{
		var halfContainerSize = spawnArea.size / 2;
		var randomX = Random.Range(-halfContainerSize.x, halfContainerSize.x);
		var randomY = Random.Range(-halfContainerSize.y, halfContainerSize.y);
		var positionToTest = new Vector2(randomX, randomY);
		var hitAmount = Physics.OverlapSphereNonAlloc(positionToTest, gameConfigs.userMagnetRadius, overlayResults);
		//If we hit more than one item then try again, this is not a bad position
		if (hitAmount > 1)
		{
			currTrialIdx++;
			//20 trials and no good result, just press on a corner
			if(currTrialIdx > 20)
			{
				Debug.LogWarning("Could find a good place to hit");
				return halfContainerSize;
			}
			return GetBadMousePosition(currTrialIdx);
		}
		else return positionToTest;
	}


	void OnPicketEnergyItem(MG_MagnetsEnergyItem itemPicked)
	{
		GeneralGameAnalyticsManager.RegisterWin();
		gameUi.StarEarned(Input.mousePosition);
		itemPicked.OnWasPicked();
		audiosource.clip = capturedItemSfx;
		audiosource.Play();
        magnetsCollected++;
		currEnergyPicked++;
		inGame_currPointsTextUI.text = currEnergyPicked.ToString();
		currEneryProgress = currEnergyPicked;
		currEneryProgress /= gameConfigs.neededEnergyToPick;
		EnergyFillImage.fillAmount = currEneryProgress;
		var won = Mathf.Abs(currEneryProgress - 1f) < 0.02f;
		if (won) OnGameOver();   
	}


	void SpawnNewItem()
	{
		if (currSpawnedItems >= gameConfigs.maxSpawnsOnScreen) return;
		var newItem = energyItemsPool.GetNewItem();
		var halfContainerSize = Vector3.zero;
		
        if (!UserDataManager.CurrUser.IsTutorialStepDone(tutorialSteps.MG_Magnets_2FourItemEnergyClick)) 
			halfContainerSize = new Vector3(gameConfigs.userMagnetRadius, gameConfigs.userMagnetRadius, gameConfigs.userMagnetRadius);
		else halfContainerSize = spawnArea.size / 2;

		var randomX = Random.Range(-halfContainerSize.x, halfContainerSize.x);
		var randomY = Random.Range(-halfContainerSize.y, halfContainerSize.y);
		newItem.transform.position = new Vector2(randomX, randomY);
		newItem.Init(gameConfigs.energyItemsLifeTime, energyItemsPool);
        currSpawnedItems++;

    }

    void OnGameOver()
	{
		StartCoroutine(EndGameAfterTime());
	}

	IEnumerator EndGameAfterTime()
	{
		currEneryProgress = currEnergyPicked;
		currEneryProgress /= gameConfigs.neededEnergyToPick;
		afterActionEnergyFillImage.fillAmount = currEneryProgress;
		var won = Mathf.Abs(afterActionEnergyFillImage.fillAmount - 1f) < 0.02f;
		if (!won)
		{
            audiosource.clip = lostGameAudio;
            audiosource.Play();
            noLeftMagnetsAnims.SetTrigger("Play");
			yield return new WaitForSeconds(1);
		}
		timePlayed = totalTime;

		afterActionPanel.SetActive(true);
		ingameObj.SetActive(false);
		ingameObjUI.SetActive(false);

		winTitle.SetActive(won);
		loseTitle.SetActive(!won);
		afterAction_currPointsTextUI.SetText(currEnergyPicked.ToString());
        instructionSource.clip = noStarsAudio;
        instructionSource.Play();
        yield return new WaitForSeconds(noStarsAudio.length);
        gameConfigs.coinsCollected = currEnergyPicked;
		eogManager.OnGameOver();
	}

}
