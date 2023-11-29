using System.Collections;
using System.Collections.Generic;
using UnityEditor.AssetImporters;
using UnityEngine;

[CreateAssetMenu(fileName = "BoostersAndScape_GameConfig", menuName = "MiniGames/BoostersAndScape_GameConfig")]
public class MG_BoostersAndScape_GameConfig : ScriptableObject
{
    public float turboSpeed;
    public float accelerationSpeed;
    public float regularSpeed;
    public int boostersPerRun;
    public int forcedFails;
    public float boosterTriggerRate;
}
