using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    public Transform bkSize;
    PlayerController player => PlayerController.Instance;
    StarsSpawner starsSpawner;
    public void Init()
    {
        starsSpawner = GetComponentInChildren<StarsSpawner>();
        var newSize = bkSize.localScale;
        newSize.x = player.levelConfig.regularRideDuration * player.levelConfig.regularSpeed;
        bkSize.localScale = newSize;
        starsSpawner.Init();
    }
    public void EndOfGame()
    {
        for (int i = 0; i < starsSpawner.stars.Count; i++)
        {
            starsSpawner.stars[i].Deactivate();
        }
    }

}
