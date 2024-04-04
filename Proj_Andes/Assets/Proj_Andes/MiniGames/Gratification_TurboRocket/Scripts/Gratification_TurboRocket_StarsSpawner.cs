using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Gratification_TurboRocket_StarsSpawner : MonoBehaviour
{
    iTurboRocketManager player => iTurboRocketManager.Instance;
    public BoxCollider spawnArea;
    public Gratification_TurboRocket_StarsController starSample;
    public Gratification_TurboRocket_BackgroundController backgroundController;
    public List<Gratification_TurboRocket_StarsController> stars = new List<Gratification_TurboRocket_StarsController>();

    public BoxCollider SpawnArea => spawnArea;
    public virtual void Init()
    {
        //backgroundController = GetComponentInParent<Gratification_TurboRocket_BackgroundController>();
        var newSize = spawnArea.size;
        newSize.x = backgroundController.bkSize.transform.localScale.x * 0.9f;
        spawnArea.size = newSize;
        var spawnSpot = spawnArea.size.x/player.starsAmt;
        //Alittle bit more than 2 so that the first star doesn't appear in the face of the player
        var offset = spawnArea.size.x / 2.2f;
        var center = spawnArea.transform.position;
        var upLimit = center + spawnArea.size/2;
        var downLimit = center - spawnArea.size/2;
        for (int i = 0; i < player.levelConfig.starsAmount; i++)
        {
			var randomPos = center;
			var newStar = GetStar();
            randomPos.x = -offset + spawnSpot * i;
            randomPos.y = center.y + Random.Range(downLimit.y, upLimit.y);
            newStar.transform.position = randomPos;
            stars.Add(newStar);
        }
    }


    Gratification_TurboRocket_StarsController GetStar()
    {
        return Instantiate(starSample, spawnArea.transform);
    }
}
