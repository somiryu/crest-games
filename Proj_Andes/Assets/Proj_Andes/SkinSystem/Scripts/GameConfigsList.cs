using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfigsList", menuName = "GameConfigs/GameConfigsList")]
public class GameConfigsList : ScriptableObject
{
    public static string instancePath = "GameConfigsList";
    static GameConfigsList instance;
    public static GameConfigsList Instance
    {
        get
        {
            if (!instance) instance = Resources.Load<GameConfigsList>(instancePath);
            return instance;
        }
    }

    public List<GameConfig> gameConfigs;
    public int goToSceneIdx;
    public GameConfig GetCurrentGame()
    {
        return gameConfigs[goToSceneIdx];
    }
}
