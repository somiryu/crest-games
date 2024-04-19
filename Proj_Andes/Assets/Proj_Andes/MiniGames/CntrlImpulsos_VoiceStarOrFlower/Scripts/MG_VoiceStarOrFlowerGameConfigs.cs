using System;
using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "VoiceStarOrFlowerGameConfigs", menuName = "MiniGames/VoiceStarOrFlowerGameConfigs")]
public class MG_VoiceStarOrFlowerGameConfigs : GameConfig
{
    public VoiceOrImageGameType gameType;

    public static string CurrFlowersAndCloudsGameID;
    public static GeneralGameAnalytics GlobalGeneralGameAnalytics;
    public bool isLastFlowerAndCloudGameOnBatch = false;

    public float timePerChoice = 5f;
	public int maxRounds = 10;
	public int initialCoins = 0;
	public int coinsOnCorrectAnswer = 0;
	public int coinsOnWrongAnswer = 0;
	public float intermediateRoundHold;
    public int finalTutoStepMaxFailuresBeforeSkipping;
    public bool testIsOppositeToStimuli;
    public static int passedTuto1 = -1;
    public static int passedTuto2 = -1;
    public static int passedTuto3 = -1;
    public override string GetSceneID() => DataIds.voiceStarGame;

    public static bool UseVoiceAsTheCorrectAnswer = true;
    public int gameIdx;

    public override SimpleGameSequenceItem GetNextItem()
    {
        var currItem = base.GetNextItem();
        if (currItem != null)
        {
            MG_VoiceStarOrFlowerManager.currTutoStepIdx = gameIdx;
        }
        return currItem;
    }
    public override void SaveAnalytics()
    {
        if (string.IsNullOrEmpty(CurrFlowersAndCloudsGameID))
        {
            CurrFlowersAndCloudsGameID = Guid.NewGuid().ToString();
            GlobalGeneralGameAnalytics = new GeneralGameAnalytics();
        }

        GameID = CurrFlowersAndCloudsGameID;
        shouldTryToSaveGeneralAnalytics = isLastFlowerAndCloudGameOnBatch;

        Debug.Log("Will save general game analytics: " + shouldTryToSaveGeneralAnalytics);

        if (!shouldTryToSaveGeneralAnalytics)
        {
            GlobalGeneralGameAnalytics.CopyFrom(GeneralGameAnalyticsManager.Instance.analytics);
        }

        var currAnalyticsDictionary = new Dictionary<string, object>();
        var currAnalytics = MG_VoiceStarOrFlowerManager.Instance.AllRoundsAnalytics;
		GameID = Guid.NewGuid().ToString();
		for (int i = 0; i < currAnalytics.Count; i++)
        {
            currAnalyticsDictionary.Clear();
            currAnalyticsDictionary.Add(DataIds.GameID, GameID);
            currAnalyticsDictionary.Add(DataIds.institutionCode, UserDataManager.CurrUser.institutionCode);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerFrustrationMode, 0);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerPassedTuto1, passedTuto1);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerPassedTuto2, passedTuto2);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerPassedTuto3, passedTuto3);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerTrial, currAnalytics[i].roundCount);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerAudibleStimuli, currAnalytics[i].audio);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerVisualStimuli, currAnalytics[i].image);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerAnswer, currAnalytics[i].selection);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerCode, currAnalytics[i].wonRound);
            currAnalyticsDictionary.Add(DataIds.responseTime, currAnalytics[i].timeToMakeAChoice);

			UserDataManager.SaveUserAnayticsPerGame(DataIds.voiceStarGame, currAnalyticsDictionary);
            Debug.Log("to test tuto pass " + passedTuto1 + " " + passedTuto2 + " " + passedTuto3);
		}
    }
    public override void AferGeneralAnalyticsSaved()
    {
        if (!isLastFlowerAndCloudGameOnBatch) return;

        CurrFlowersAndCloudsGameID = null;
        GlobalGeneralGameAnalytics = new GeneralGameAnalytics();
    }
    public override void ResetCurrentAnalytics()
    {
        CurrFlowersAndCloudsGameID = null;
        GlobalGeneralGameAnalytics = new GeneralGameAnalytics();
    }
}

