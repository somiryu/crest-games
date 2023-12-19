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
    [Space]
    public bool randomize;
    [Tooltip("How many games should we play before leaving the group (Only works if randomize is enable), if -1 we will play all items in the group")]
    public int maxItemsToPlayOnRandomize = -1;
    public GameSequenceItem GetNextMiniGame()
    {
        if (randomize)
        {
            return GetRandomGame();
        }
        if(miniGamesInGroup.Contains(GameSequencesList.Instance.prevGame))
        {
            var lastGameIdx = miniGamesInGroup.IndexOf(GameSequencesList.Instance.prevGame);
            var newGameIdx = lastGameIdx + 1;

            if (newGameIdx >= miniGamesInGroup.Count) return null;

            var newGame = miniGamesInGroup[lastGameIdx + 1];
            GameSequencesList.Instance.prevGame = newGame;

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
        var maxItemsToPlay = maxItemsToPlayOnRandomize != -1 ? maxItemsToPlayOnRandomize : miniGamesInGroup.Count;
        if(itemsPlayed.Count >= maxItemsToPlay) return null;

        var newGame = miniGamesInGroup[Random.Range(0, miniGamesInGroup.Count)];
        if (!itemsPlayed.Contains(newGame))
        {
            GameSequencesList.Instance.prevGame = newGame;
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
        return GetNextMiniGame();
    }

    public override void OnReset()
    {
        itemsPlayed.Clear();
    }
}
