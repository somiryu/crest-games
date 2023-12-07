using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "MiniGameGroup", menuName = "GameConfigs/MiniGameGroup")]
public class MinigameGroups : GameSequence
{
    public List<GameSequenceItem> miniGamesInGroup;
    public bool randomize;
    GameSequenceItem prevGame = GameSequencesList.Instance.prevGame;
    public GameSequenceItem GetNextMiniGame()
    {
        if (randomize)
        {
            return GetRandomGame();
        }
        if(miniGamesInGroup.Contains(prevGame))
        {
            var newGame = miniGamesInGroup[miniGamesInGroup.IndexOf(prevGame) + 1];
            prevGame = newGame;
            return newGame;
        }
        else
        {
            prevGame = miniGamesInGroup[0];
            return miniGamesInGroup[0];
        }
    }

    GameSequenceItem GetRandomGame()
    {
        var newGame = miniGamesInGroup[Random.Range(0, miniGamesInGroup.Count)];
        if (newGame != prevGame)
        {
            return newGame;
        }
        else return GetRandomGame();
    }

    public override void OnSequenceStart()
    {
        throw new System.NotImplementedException();
    }

    public override void OnSequenceOver()
    {
        GameSequencesList.Instance.GoToNextSequence();
    }

    public override GameSequenceItem GetNextItem()
    {
        return GetNextMiniGame();
    }
}
