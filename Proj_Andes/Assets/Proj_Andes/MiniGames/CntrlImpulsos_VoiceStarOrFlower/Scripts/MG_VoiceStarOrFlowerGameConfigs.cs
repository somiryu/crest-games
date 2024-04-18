using System;
using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "VoiceStarOrFlowerGameConfigs", menuName = "MiniGames/VoiceStarOrFlowerGameConfigs")]
public class MG_VoiceStarOrFlowerGameConfigs : GameConfig
{
    public VoiceOrImageGameType gameType;
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
    public override string GetSceneID() => DataIds.voiceStarGame;

    /// <summary>
    /// If false, the game mechanic will use the image as the correct answer and voice as the wrong one
    /// </summary>
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
    public override void ResetCurrentAnalytics()
    {
        base.ResetCurrentAnalytics();
    }
}

