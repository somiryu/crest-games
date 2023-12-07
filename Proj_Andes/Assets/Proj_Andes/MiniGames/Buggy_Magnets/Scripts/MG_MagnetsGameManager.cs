using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MG_MagnetsGameManager : MonoBehaviour
{
    public Pool<MG_MagnetsEnergyItem> energyItemsPool;


	[SerializeField] MG_MagnetsConfigs gameConfigs;
	[SerializeField] BoxCollider spawnArea;
	[SerializeField] MG_MagnetRangeHandler magnetRangeIndicator;

	[Header("Game UI")]
	[SerializeField] Image EnergyFillImage;
	[SerializeField] TMP_Text MagnetsAmount;

	[Header("after action UI")]
	[SerializeField] GameObject afterActionPanel;
	[SerializeField] Image afterActionEnergyFillImage;
	[SerializeField] GameObject winTitle;
	[SerializeField] GameObject loseTitle;
	[SerializeField] Button retryBtn;

	private float timer;
	private int currSpawnedItems;
	private int availableMagnets;
	private int currEnergyPicked;
	private float currEneryProgress;

	private Collider[] overlayResults = new Collider[20];
	

	public void Awake()
	{
		Init();
	}

	public void Init()
    {
		energyItemsPool.Init(30);
		availableMagnets = gameConfigs.initialMagnetsCount;
		timer = 0;
		currSpawnedItems = 0;
		currEnergyPicked = 0;
		currEneryProgress = 0;
        retryBtn.onClick.AddListener(() => SceneManagement.GoToScene(gameConfigs.scene));
		EnergyFillImage.fillAmount = 0;
		MagnetsAmount.SetText(availableMagnets.ToString());
		magnetRangeIndicator.Init(gameConfigs.userMagnetRadius);
	}

	private void Update()
	{
		timer += Time.deltaTime;
		if(timer > gameConfigs.timeBetweenSpawns)
		{
			timer = 0;
			SpawnNewItem();
		}

		if(Input.GetMouseButtonDown(0))
		{
			var mouseGlobalPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			mouseGlobalPosition.z = 0;
			if (gameConfigs.activeCheats && currEneryProgress >= 0.5f)
			{
				mouseGlobalPosition = GetBadMousePosition(0);
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
			MagnetsAmount.SetText(availableMagnets.ToString());
			if(availableMagnets == 0)
			{
				OnGameOver();
			}
		}
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
		energyItemsPool.RecycleItem(itemPicked);
		currEnergyPicked++;
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
	}

	void OnGameOver()
	{
		afterActionPanel.SetActive(true);
		currEneryProgress = currEnergyPicked;
		currEneryProgress /= gameConfigs.neededEnergyToPick;
		afterActionEnergyFillImage.fillAmount = currEneryProgress;
		var won = Mathf.Abs(afterActionEnergyFillImage.fillAmount - 1f) < 0.02f;
		winTitle.SetActive(won);
		loseTitle.SetActive(!won);
	}

}
