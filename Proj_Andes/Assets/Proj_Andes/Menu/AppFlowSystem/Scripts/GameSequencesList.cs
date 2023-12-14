using System;
using System.Collections.Generic;
using Tymski;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "GameSequencesList", menuName = "GameSequencesList/GameSequencesList")]
public class GameSequencesList : ScriptableObject
{
    public static string instancePath = "GameSequencesList";
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
    [NonSerialized] public GameSequenceItem prevGame;

    [NonSerialized] public int goToGameGroupIdx;
    public bool continueToNextItem;
    public void OnValidate()
    {
        if (continueToNextItem)
        {
            continueToNextItem = false;
            SceneManagement.GoToScene(GetGameSequence().GetNextItem().scene);
        }
    }
    public void GoToNextItemInList()
    {
        SceneManagement.GoToScene(GetGameSequence().GetNextItem().scene);
    }
    [ContextMenu("ResetSequence")]
    public void ResetSequence()
    {
        prevGame = null;
        goToGameGroupIdx = 0;
        for (int i = 0; i < gameSequences.Count; i++) gameSequences[i].OnReset();
    }
    public GameSequence GetGameSequence()
    {
        return gameSequences[goToGameGroupIdx];
    }

    public void GoToNextSequence()
    {
        if (goToGameGroupIdx < gameSequences.Count - 1) 
        {
            goToGameGroupIdx++;
            prevGame = null;
            SceneManagement.GoToScene(GetGameSequence().GetNextItem().scene);
        } 
        else Debug.Log("End of sequences");
    }

}
public abstract class GameSequence : GameSequenceItem
{
    public abstract void OnReset();
    public abstract GameSequenceItem GetNextItem();
    public abstract void OnSequenceOver();
}

public class GameSequenceItem : ScriptableObject
{
    public SceneReference scene;
}

public abstract class GameConfig : GameSequenceItem
{


}

