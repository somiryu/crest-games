using System;
using System.Collections;
using System.Collections.Generic;
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
        for (int i = 0; i < modifierPerDifficultLevel.Count; i++)
        {
            Modifier currentModifier = modifierPerDifficultLevel[i];

            if (currentModifier.difficultyLevel == UserDataManager.CurrUser.difficultyLevel)
            {
                switch (currentModifier.difficultyOperation)
                {
                    case DifficultyOperation.Multiply:
                        return valueBase * currentModifier.valueModificator;
                    case DifficultyOperation.Add:
                        return valueBase + currentModifier.valueModificator;
                    case DifficultyOperation.Subtract:
                        return valueBase - currentModifier.valueModificator;
                }
            }
        }
        return valueBase;
    }
}

[Serializable]
public class Modifier
{
    public DifficultyLevel difficultyLevel;
    public DifficultyOperation difficultyOperation;
    public float valueModificator;
}

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard,
    NONE
}

public enum DifficultyOperation
{
    Add,
    Subtract,
    Multiply
}
