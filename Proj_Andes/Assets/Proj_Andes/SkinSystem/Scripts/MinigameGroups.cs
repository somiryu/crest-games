using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "MiniGameGroup", menuName = "GameConfigs/MiniGameGroup")]
public class MinigameGroups : ScriptableObject
{
    public List<GameConfig> miniGamesInGroup;
    public bool randomize;
    public GameConfig prevGame;
    public GameConfig currGame;

    public GameConfig GetNextMiniGame()
    {
        if (randomize)
        {
            var newRandomGame = GetRandomGame();
            return newRandomGame;
        }
        if(miniGamesInGroup.Contains(prevGame))
        {
            prevGame = miniGamesInGroup[miniGamesInGroup.IndexOf(prevGame)];
            Debug.Log(GetNextMiniGame().ToString() + " " + miniGamesInGroup[miniGamesInGroup.IndexOf(prevGame)]);
            return miniGamesInGroup[miniGamesInGroup.IndexOf(prevGame)+1];
        }
        else
        {
            Debug.Log(GetNextMiniGame().ToString() + " " + miniGamesInGroup[miniGamesInGroup.IndexOf(prevGame)]);
            return miniGamesInGroup[0];
        }
    }

    GameConfig GetRandomGame()
    {
        return miniGamesInGroup[ Random.Range(0, miniGamesInGroup.Count)];
    }
}
