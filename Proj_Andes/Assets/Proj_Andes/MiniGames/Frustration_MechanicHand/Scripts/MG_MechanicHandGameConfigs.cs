using System;
using System.Collections.Generic;
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
	public static int repetition = 0;

    public override string GetSceneID() => DataIds.mechanicHandGame;
    public override void SaveAnalytics()
    {
		repetition++;
        var currAnalytics = MG_MechanicHand_GameManger.Instance.allRoundAnalytics;
		GameID = Guid.NewGuid().ToString();
        var currAnalyticsDictionary = new Dictionary<string, object>();

        itemAnalytics = new Dictionary<string, object>();
		UserDataManager.LastDocumentIDsStored = new List<string>();

		for (int i = 0; i < currAnalytics.Count; i++)
		{
            currAnalyticsDictionary.Clear();

            var currData = currAnalytics[i];
            currAnalyticsDictionary.Add(DataIds.GameID, GameID);
            currAnalyticsDictionary.Add(DataIds.mechHandTest, repetition);
            currAnalyticsDictionary.Add(DataIds.mechHandTrial, currData.roundCount);
            currAnalyticsDictionary.Add(DataIds.mechHandThrown, currData.thrown);
            currAnalyticsDictionary.Add(DataIds.mechHandPresicion, currData.presition);

            var newDocID = Guid.NewGuid().ToString();

            UserDataManager.LastCollectionIDStored = DataIds.mechanicHandGame;
			UserDataManager.LastDocumentIDsStored.Add(newDocID);

            UserDataManager.SaveUserAnayticsPerGame(DataIds.mechanicHandGame, currAnalyticsDictionary, newDocID);
        }

		SetPostFrustration();
	}

	public static void SetPostFrustration()
	{
		MG_SizeRockets_GameConfigs.postFrustration = 1;
		MG_VoiceStarOrFlowerGameConfigs.postFrustration = 1;
		MG_HearthAndStarsGameConfigs.postFrustration = 1;
		Gratification_TurboRocket_GameConfig.postFrustration = 1;
	}
    public override void ResetCurrentAnalytics()
    {
        base.ResetCurrentAnalytics();
    }
}
