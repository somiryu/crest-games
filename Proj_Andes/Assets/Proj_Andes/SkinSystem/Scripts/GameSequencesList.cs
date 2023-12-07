using System.Collections.Generic;
using Tymski;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "GameConfigsList", menuName = "GameConfigs/GameConfigsList")]
public class GameSequencesList : ScriptableObject
{
    public static string instancePath = "GameConfigsList";
    static GameSequencesList instance;
    public static GameSequencesList Instance
    {
        get
        {
            if (!instance) instance = Resources.Load<GameSequencesList>(instancePath);
            return instance;
        }
    }

    public List<GameSequence> gameSequences;
    public GameSequenceItem prevGame;
    public int goToGameGroupIdx;
    public bool continueToNextItem;
    public void OnValidate()
    {
        if (continueToNextItem)
        {
            continueToNextItem = false;
            SceneManagement.GoToScene(GetGameSequence().GetNextItem().scene);
        }
    }
    [ContextMenu("ResetSequence")]
    private void ResetSequence()
    {
        prevGame = null;
    }
    public GameSequence GetGameSequence()
    {
        return gameSequences[goToGameGroupIdx];
    }

    public void GoToNextSequence()
    {
        goToGameGroupIdx++;
        //SceneManagement.GoToScene(GetGameSequence().GetNextItem().scene);
    }

}

public abstract class GameConfig : GameSequenceItem
{
    //public abstract void OnGameOver();
}

public abstract class GameSequence : ScriptableObject
{
    public abstract void OnSequenceStart();
    public abstract GameSequenceItem GetNextItem();
    public abstract void OnSequenceOver();
}
public abstract class GameSequenceItem : ScriptableObject
{
    public abstract SceneReference scene { get; }

}
