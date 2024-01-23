using System;
using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "MG_MechanicHandGameConfigs", menuName = "MiniGames/MG_MechanicHandGameConfigs")]
public class MG_MechanicHandGameConfigs : GameConfig
{
	public int asteroidsPerRound = 5;
	public float asteroidsSizeLoseAmountPerRound = 0.2f;
	public int playerLifes = 5;
	public float hookRotationSpeed = 1;
	public float hookMoveSpeed = 1;
	[Range(0f,1f)]
	public float percentageNeededToWin = 0.8f;
	public bool activeCheats;

    [NonSerialized] public int totalSuccessfulAttempts;
    [NonSerialized] public int totalFailedAttempts;
    public override void SaveAnalytics()
    {
        itemAnalytics = new Dictionary<string, object>();
        itemAnalytics.Add(DataIds.voiceStarTotalGametime, totalSuccessfulAttempts);
        itemAnalytics.Add(DataIds.voiceStarRoundResultWins, totalSuccessfulAttempts);
    }
    public override void ResetCurrentAnalytics()
    {
        totalSuccessfulAttempts = 0; 
        totalFailedAttempts = 0;
        base.ResetCurrentAnalytics();
    }
}
