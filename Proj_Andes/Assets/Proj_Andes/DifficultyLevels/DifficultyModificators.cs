using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;

public class DifficultyModificators : MonoBehaviour
{   
}


[Serializable]
public class DifficultyModificatorFloat
{
    public float valueBase;
    public List<Modifier> modifierPerDifficultLevel;

    public float GetValueModify()
    {
        var difficulty = UserDataManager.Instance.GetDifficultyLevelUser();
        Modifier currentModifier = GetCurrentModifier(difficulty);
        return currentModifier.ApplyModification(valueBase);        
    }
    public Modifier GetCurrentModifier(DifficultyLevel difficultyLevel)
    {
        for (int i = 0; i < modifierPerDifficultLevel.Count; i++)
        {
            Modifier currentModifier = modifierPerDifficultLevel[i];
            if (currentModifier.difficultyLevel == difficultyLevel) return currentModifier;
        }
        Debug.Log("No modifier found for the current difficulty level.");
        return null;
    }
}

[Serializable]
public class Modifier
{
    public DifficultyLevel difficultyLevel;
    public DifficultyOperation difficultyOperation;
    public float valueModificator;

    public float ApplyModification(float valueBase)
    {        
        switch (difficultyOperation)
        {
            case DifficultyOperation.Multiply:
                return valueBase * valueModificator;
            case DifficultyOperation.Add:
                return valueBase + valueModificator;
            case DifficultyOperation.Subtract:
                return valueBase - valueModificator;
        }
        return valueBase;
    }

}

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}

public enum DifficultyOperation
{
    Add,
    Subtract,
    Multiply
}
