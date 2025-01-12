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
    public static int passedTuto1 = 0;
    public static int passedTuto2 = 0;
    public static int passedTuto3 = 0;
    public static int postFrustration = 0;

    public override string GetSceneID() => DataIds.voiceStarGame;

    public static bool UseVoiceAsTheCorrectAnswer = true;
    public static bool OverridenUseVoiceAsTheCorrectAnswer = true;
    public static bool Overwritten = false;
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
        Debug.Log("voice id bef" + CurrFlowersAndCloudsGameID );
        if (string.IsNullOrEmpty(CurrFlowersAndCloudsGameID))
        {
            CurrFlowersAndCloudsGameID = Guid.NewGuid().ToString();
            GlobalGeneralGameAnalytics = new GeneralGameAnalytics();
        }

        GameID = CurrFlowersAndCloudsGameID;
        shouldTryToSaveGeneralAnalytics = isLastFlowerAndCloudGameOnBatch;

        Debug.Log("voice id " + CurrFlowersAndCloudsGameID + " Will save general game analytics: " + shouldTryToSaveGeneralAnalytics);

        if (!shouldTryToSaveGeneralAnalytics)
        {
            GlobalGeneralGameAnalytics.CopyFrom(GeneralGameAnalyticsManager.Instance.analytics);
        }

        var currAnalyticsDictionary = new Dictionary<string, object>();
        var currAnalytics = MG_VoiceStarOrFlowerManager.Instance.AllRoundsAnalytics;

		for (int i = 0; i < currAnalytics.Count; i++)
        {
            currAnalyticsDictionary.Clear();
            currAnalyticsDictionary.Add(DataIds.GameID, GameID);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerTest, (int)gameType);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerFrustrationMode, postFrustration);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerPassedTuto1, passedTuto1);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerPassedTuto2, passedTuto2);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerPassedTuto3, passedTuto3);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerTrial, currAnalytics[i].roundCount);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerAudibleStimuli, currAnalytics[i].audio);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerVisualStimuli, currAnalytics[i].image);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerAnswer, currAnalytics[i].selection);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerCode, currAnalytics[i].wonRound);
            currAnalyticsDictionary.Add(DataIds.cloudNFlowerTimer, currAnalytics[i].timeToMakeAChoice);

			UserDataManager.SaveUserAnayticsPerGame(DataIds.voiceStarGame, currAnalyticsDictionary);
		}
        Debug.Log("post frust voice" + postFrustration);
    }
    public override void AferGeneralAnalyticsSaved()
    {
        if (!isLastFlowerAndCloudGameOnBatch) return;

        CurrFlowersAndCloudsGameID = null;
        GlobalGeneralGameAnalytics = new GeneralGameAnalytics();
    }
    public override void ResetCurrentAnalytics()
    {
        if (!isLastFlowerAndCloudGameOnBatch) return;

        CurrFlowersAndCloudsGameID = null;
        GlobalGeneralGameAnalytics = new GeneralGameAnalytics();
    }
}

public enum VoiceOrImage
{
    VoiceDominant,
    ImageDominant
}