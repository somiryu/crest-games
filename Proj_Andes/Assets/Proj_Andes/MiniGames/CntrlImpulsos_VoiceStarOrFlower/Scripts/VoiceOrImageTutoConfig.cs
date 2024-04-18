using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VoiceOrImageTutoConfig", menuName = "TutorialConfig/VoiceOrImageTutoConfig")]
public class VoiceOrImageTutoConfig : SimpleGameSequenceItemTutorial
{
    public VoiceOrImageGameType gameType;
    public int gameIdx;
    public int trialsAmt;
    public int consecutiveWinsToPass;
    public int consecutiveFailsToLose;
    public float roundTime;
    [Header("First Instructions")]
    public AudioClip firstInstructionAudio;
    public AudioClip firstInstructionAudio2;
    [Header("Second Instructions")]
    public AudioClip secondInstructionAudio1;
    public AudioClip secondInstructionAudio2;
    [Header("Completed tutorial audio")]
    public AudioClip endAudio;
    public bool switchesToAnswerIsDifferentInSecondPart;
    public bool completedFirstPart;
    public override SimpleGameSequenceItem GetNextItem()
    {
        var currItem = base.GetNextItem();
        if (currItem != null)
        {
            MG_VoiceStarOrFlowerManagerTutorial.currTutoStepIdx = gameIdx;
        }
        return currItem;
    }
}
public enum VoiceOrImageGameType
{
    Voice,
    Image,
    Mixed
}