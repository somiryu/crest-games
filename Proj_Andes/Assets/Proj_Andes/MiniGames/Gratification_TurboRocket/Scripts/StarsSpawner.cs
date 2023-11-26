using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StarsSpawner : MonoBehaviour
{
    PlayerController player => PlayerController.Instance;
    [SerializeField] BoxCollider spawnArea;
    [SerializeField] StarsController starSample;
    BackgroundController backgroundController;
    public List<StarsController> stars = new List<StarsController>();

    public BoxCollider SpawnArea => spawnArea;
    public void Init()
    {
        backgroundController = GetComponentInParent<BackgroundController>();
        var newSize = spawnArea.size;
        newSize.x = backgroundController.bkSize.transform.localScale.x * 0.9f;
        spawnArea.size = newSize;
        var spawnSpot = spawnArea.size.x/player.levelConfig.starsAmount;
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


    StarsController GetStar()
    {
        return Instantiate(starSample, spawnArea.transform);
    }
}
