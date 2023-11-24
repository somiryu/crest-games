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
    public void Init()
    {
        backgroundController = GetComponentInParent<BackgroundController>();
        var newSize = spawnArea.size;
        newSize.x = backgroundController.bkSize.transform.localScale.x;
        spawnArea.size = newSize;
        var spawnSpot = spawnArea.size.x/player.levelConfig.starsAmount;
        var offset = spawnArea.size.x / 2;
        var center = spawnArea.bounds.center;
        var upLimit = center + spawnArea.bounds.extents;
        var downLimit = center - spawnArea.bounds.extents;
        var randomPos = center;
        for (int i = 0; i < player.levelConfig.starsAmount; i++)
        {
            var newStar = GetStar();
            randomPos.x = -offset + spawnSpot * i;
            randomPos.y += Random.Range(downLimit.y+2, upLimit.y-2);
            newStar.transform.position = randomPos;
            stars.Add(newStar);
        }
    }
    void Update()
    {
        if (PlayerController.Instance.gameStages == GameStages.End) return;

    }
    StarsController GetStar()
    {
        return Instantiate(starSample, spawnArea.transform);
    }
}
