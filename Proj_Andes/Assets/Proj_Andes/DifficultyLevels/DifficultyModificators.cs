using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

public class DifficultyModificators : MonoBehaviour
{   
}

[Serializable]
public class DifficultyModificatorInt
{
    public DifficultyLevel difficultyLevel;
    public int valueBase;
    public DifficultyOperation difficultyOperation;
    public int valueModificator;

    public int GetValueModify()
    {
        return 0;
    }

}

[Serializable]
public class DifficultyModificatorFloat
{
    public DifficultyLevel difficultyLevel;
    public float valueBase;
    public DifficultyOperation difficultyOperation;
    public float valueModificator;

    public int GetValueModify()
    {
        return 0;
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
