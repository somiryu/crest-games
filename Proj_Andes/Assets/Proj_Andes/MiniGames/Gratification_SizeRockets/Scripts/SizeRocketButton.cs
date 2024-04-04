using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeRocketButton : MonoBehaviour
{
    MG_SizeRockets_GameConfigs gameConfigs => ISizeRocketsManager.Instance.gameConfigs;
    [SerializeField] SizeRocketsRocketTypes rocketTypes;
    ObtainableStarsController obtainableStarsController;
    void Start()
    {
        obtainableStarsController = GetComponentInChildren<ObtainableStarsController>();
        for (int i = 0; i < gameConfigs.shipsConfigs.Length; i++)
        {
            if (gameConfigs.shipsConfigs[i].rocketType == rocketTypes) obtainableStarsController.ActivateStars(gameConfigs.shipsConfigs[i].coinsCapacity);
        }
    }

}
