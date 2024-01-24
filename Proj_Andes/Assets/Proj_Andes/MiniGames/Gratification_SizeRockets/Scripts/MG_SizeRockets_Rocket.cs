using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_SizeRockets_Rocket : MonoBehaviour
{
    public SizeRocketsRocketTypes rocketType;
	public SizeRocketsTravelState state;

    public float speed;
    public int coinsCapacity;

    public MG_SizeRockets_Planet targetPlanet;
    public Transform basePlanet;
	Pool<MG_SizeRockets_Rocket> pool;

	public int coinsCarrying;

	public void Init(
		Pool<MG_SizeRockets_Rocket> _pool,
		MG_SizeRockets_Planet _targetPlanet, 
		Transform _basePlanet)
	{
		targetPlanet = _targetPlanet;
		basePlanet = _basePlanet;
		state = SizeRocketsTravelState.GoingToPlanet;
		pool = _pool;

		var config = ISizeRocketsManager.Instance.gameConfigs.GetShipConfig(rocketType);

        speed = config.speed;
		coinsCapacity = config.coinsCapacity;

		coinsCarrying = 0;
	}


	private void Update()
	{
		if(state == SizeRocketsTravelState.GoingToPlanet)
		{
			transform.position = Vector3.MoveTowards(transform.position, targetPlanet.transform.position, speed * Time.deltaTime);

			var currDist = transform.position - targetPlanet.transform.position;
			if(currDist.magnitude < 0.1f)
			{
				coinsCarrying = Mathf.Min(coinsCapacity, targetPlanet.coinsAmount);
				targetPlanet.UpdateCoinsAmount(targetPlanet.coinsAmount - coinsCarrying);
				state = SizeRocketsTravelState.BackToBase;
			}
		}
		else if(state == SizeRocketsTravelState.BackToBase)
		{
			transform.position = Vector3.MoveTowards(transform.position, basePlanet.position, speed * Time.deltaTime);
			var currDist = transform.position - basePlanet.position;
			if (currDist.magnitude < 0.1f)
			{
				ISizeRocketsManager.Instance.OnShipDeliveredCoins(this, coinsCarrying);
				pool.RecycleItem(this);
			}
		}
	}

}

public enum SizeRocketsTravelState
{
	GoingToPlanet,
	BackToBase
}
