using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MG_MagnetsGameManager : MonoBehaviour, IEndOfGameManager
{
    public Pool<MG_MagnetsEnergyItem> energyItemsPool;


	[SerializeField] MG_MagnetsConfigs gameConfigs;
	[SerializeField] BoxCollider spawnArea;
	[SerializeField] MG_MagnetRangeHandler magnetRangeIndicator;
	[SerializeField] AudioSource lostGameAudio;

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


	private float timer;
	private int currSpawnedItems;
	private int availableMagnets;
	private int currEnergyPicked;
	private float currEneryProgress;

	private Collider[] overlayResults = new Collider[20];
	[SerializeField] EndOfGameManager eogManager;
	public EndOfGameManager EndOfGameManager => eogManager;
    public void Awake()
	{
		Init();
	}

	public void Init()
    {

		ingameObj.SetActive(true);
		ingameObjUI.SetActive(true);

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

	private void Update()
	{
		if (availableMagnets == 0) return;
		timer += Time.deltaTime;
		if (timer > gameConfigs.timeBetweenSpawnsPerDifficultLevel.GetValueModify())
		{
			timer = 0;
			for (int i = 0; i < gameConfigs.itemsAmountToSpawn; i++) SpawnNewItem();
		}

		if(Input.GetMouseButtonDown(0))
		{
			var mouseGlobalPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			mouseGlobalPosition.z = 0;
			if (gameConfigs.activeCheats && currEneryProgress >= 0.5f)
			{
				mouseGlobalPosition = GetBadMousePosition(0);
				StartCoroutine(ShowTrapSign());
			}

			magnetRangeIndicator.ShowAt(mouseGlobalPosition);

			var hitAmount = Physics.OverlapSphereNonAlloc(mouseGlobalPosition, gameConfigs.userMagnetRadius, overlayResults);

			for (int i = 0; i < hitAmount; i++)
			{
				var curr = overlayResults[i];
				if (!curr.TryGetComponent(out MG_MagnetsEnergyItem energyItem)) continue;
				OnPicketEnergyItem(energyItem);
				
			}
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
		itemPicked.OnWasPicked();
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
		currSpawnedItems++;
		var newItem = energyItemsPool.GetNewItem();
		var halfContainerSize = spawnArea.size / 2;
		var randomX = Random.Range(-halfContainerSize.x, halfContainerSize.x);
		var randomY = Random.Range(-halfContainerSize.y, halfContainerSize.y);
		newItem.transform.position = new Vector2(randomX, randomY);
		newItem.Init(gameConfigs.energyItemsLifeTime, energyItemsPool);
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
			lostGameAudio.Play();
			noLeftMagnetsAnims.SetTrigger("Play");
			yield return new WaitForSeconds(1);
		}
		
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
