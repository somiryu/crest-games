using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Gratification_TurboRocket_BackgroundController : MonoBehaviour
{
    public Transform bkSize;
    iTurboRocketManager player => iTurboRocketManager.Instance;
    public Gratification_TurboRocket_StarsSpawner starsSpawner;
    public void Init()
    {
        var newSize = bkSize.localScale;
        newSize.x = player.gameDuration * player.levelConfig.regularSpeed;
        bkSize.localScale = newSize;
        starsSpawner.Init();
        for (int i = 0; i < starsSpawner.stars.Count; i++) starsSpawner.stars[i].Init();
    }
    public void EndOfGame()
    {
        for (int i = 0; i < starsSpawner.stars.Count; i++)
        {
            starsSpawner.stars[i].Deactivate();
        }
    }

}
