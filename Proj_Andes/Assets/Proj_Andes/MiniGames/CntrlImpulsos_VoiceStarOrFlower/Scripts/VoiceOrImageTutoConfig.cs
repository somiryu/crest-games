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
    public AudioClip introAudio;
    public AudioClip introAudio2;
    public AudioClip instruction1;
    public AudioClip instruction2;
    public AudioClip endAudio;
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