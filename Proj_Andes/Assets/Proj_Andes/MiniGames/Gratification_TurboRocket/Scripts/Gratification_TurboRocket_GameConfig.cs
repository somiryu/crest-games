using System;
using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "Gratification_TurboRocket_GameConfig", menuName = "MiniGames/Gratification_TurboRocket_GameConfig")]

public class Gratification_TurboRocket_GameConfig : GameConfig
{
    public float regularSpeed;
    public float turboSpeed;
    public float accelerationSpeed;
    public float regularRideDuration;
    public int starsAmount;
    [NonSerialized] public Dictionary<string, object> analytics;
    public override Dictionary<string, object> GetAnalytics()
    {
        return genericDictionary;
    }
}
