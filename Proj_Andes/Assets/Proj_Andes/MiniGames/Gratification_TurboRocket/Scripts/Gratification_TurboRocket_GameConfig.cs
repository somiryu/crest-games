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
}
