using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "MiniGameGroup", menuName = "GameSequencesList/MiniGameGroup")]
public class MinigameGroups : GameSequence
{
    public List<GameSequenceItem> miniGamesInGroup;
    [NonSerialized] List<GameSequenceItem> itemsPlayed = new List<GameSequenceItem>();
    public bool randomize;
    public GameSequenceItem GetNextMiniGame()
    {
        if (randomize)
        {
            return GetRandomGame();
        }
        if(miniGamesInGroup.Contains(GameSequencesList.Instance.prevGame))
        {
            if(miniGamesInGroup.IndexOf(GameSequencesList.Instance.prevGame) >= miniGamesInGroup.Count - 1)
            {
                OnSequenceOver();
                return GameSequencesList.Instance.prevGame;
            }
            var newGame = miniGamesInGroup[miniGamesInGroup.IndexOf(GameSequencesList.Instance.prevGame) + 1];
            GameSequencesList.Instance.prevGame = newGame;
            Debug.Log(miniGamesInGroup.IndexOf(newGame));
            return newGame;
        }
        else
        {
            GameSequencesList.Instance.prevGame = miniGamesInGroup[0];
            return miniGamesInGroup[0];
        }
    }
    GameSequenceItem GetRandomGame()
    {
        if(itemsPlayed.Count >= miniGamesInGroup.Count)
        {
            OnSequenceOver();
            return GameSequencesList.Instance.prevGame;
        }
        var newGame = miniGamesInGroup[Random.Range(0, miniGamesInGroup.Count)];
        if (!itemsPlayed.Contains(newGame))
        {
            //GameSequencesList.Instance.prevGame = newGame;
            itemsPlayed.Add(newGame);
            return newGame;
        }
        else return GetRandomGame();
    }
    public override void OnSequenceOver()
    {
        GameSequencesList.Instance.GoToNextSequence();
    }

    public override GameSequenceItem GetNextItem()
    {
        //Debug.Log(miniGamesInGroup.IndexOf(GetNextMiniGame()));
        return GetNextMiniGame();
    }

    public override void OnReset()
    {
        itemsPlayed.Clear();
    }
}
