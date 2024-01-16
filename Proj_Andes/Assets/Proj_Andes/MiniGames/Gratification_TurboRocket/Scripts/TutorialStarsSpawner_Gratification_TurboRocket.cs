using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStarsSpawner_Gratification_TurboRocket : Gratification_TurboRocket_StarsSpawner
{
    TutorialManager_Gratification_TurboRocket tutorialManager => TutorialManager_Gratification_TurboRocket.Instance;
    [SerializeField] TutorialHand_TurboRocket hand;
    public override void Init()
    {
        var newSize = spawnArea.size;
        newSize.x = backgroundController.bkSize.transform.localScale.x * 0.9f;
        spawnArea.size = newSize;
        var spawnSpot = spawnArea.size.x / tutorialManager.tutorialConfig.starsAmount;
        //Alittle bit more than 2 so that the first star doesn't appear in the face of the player
        var offset = spawnArea.size.x / 2.2f;
        var center = spawnArea.transform.position;
        var upLimit = center + spawnArea.size / 2;
        var downLimit = center - spawnArea.size / 2;
        for (int i = 0; i < tutorialManager.tutorialConfig.starsAmount; i++)
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
    public TutorialHand_TurboRocket GetHand(Collider spawnArea)
    {
        return Instantiate(hand, spawnArea.transform);
    }
}
