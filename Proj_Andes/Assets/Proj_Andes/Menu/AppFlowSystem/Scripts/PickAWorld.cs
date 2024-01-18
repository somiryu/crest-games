using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "PickAWorld", menuName = "GameSequencesList/PickAWorld")]

public class PickAWorld : ScriptableObject
{
    public WorldsConfig worldsConfig;
    [NonSerialized] public int gameIdx;

}
