using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MG_MagnetsGameManager : MonoBehaviour, IEndOfGameManager
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

	[Header("after action UI")]
	[SerializeField] GameObject afterActionPanel;
	[SerializeField] GameObject ingameObj;
	[SerializeField] GameObject ingameObjUI;
	[SerializeField] Image afterActionEnergyFillImage;
	[SerializeField] GameObject winTitle;
	[SerializeField] GameObject loseTitle;
	[SerializeField] TMP_Text afterAction_currPointsTextUI;
	[SerializeField] AudioClip capturedItemSfx;


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

    public void Awake()
	{
		if(instance != null && instance != this) DestroyImmediate(instance);
		instance = this;
		Init();
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
	}

	private void Start()
	{
		GeneralGameAnalyticsManager.Instance.Init(DataIds.magnetsGame);
	}

	private void Update()
	{
		if (CatchCoinsAudioInstruction.Instance.firstAudio != null) return;
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
			if(gameConfigs.activeCheats && attempts > 1)
			{
				mouseGlobalPosition = GetBadMousePosition(0);
				lostByCheat++;
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

		gameConfigs.coinsCollected = currEnergyPicked;
		eogManager.OnGameOver();
	}

}
