using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level Config", menuName = "Game1/LevelConfig")]

public class Gratification_TurboRocket_LevelConfig : ScriptableObject
{
    public float regularSpeed;
    public float turboSpeed;
    public float accelerationSpeed;
    public float regularRideDuration;
    public float turboRideDuration;
    public int starsAmount;

}
