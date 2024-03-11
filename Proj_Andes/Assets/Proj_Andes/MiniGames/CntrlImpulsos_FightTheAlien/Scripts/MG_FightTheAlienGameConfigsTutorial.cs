using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MG_FightTheAlienGameConfigsTutorial", menuName = "MiniGames/MG_FightTheAlienGameConfigsTutorial")]

public class MG_FightTheAlienGameConfigsTutorial : ScriptableObject 
{ 
    public AlienAttackConfig[] alienAttacksConfigsMatch;
    public AlienAttackConfig[] alienAttacksConfigsNoMatch;
    public List<MG_FightTheAlienTutorialStep> mG_FightTheAlienTutorialSteps;

}

[Serializable]
public class MG_FightTheAlienTutorialStep
{
    public int stepsAmount;
    public alienAttacksConfigsType alienAttacksConfigsType;
    public bool helpButton;
    public bool wrongChoices;
    public bool time;
    public bool score;
    public bool life;
    public bool helpAudioFeedback;
    public AudioClip tutorialStartAudio;
}

public enum alienAttacksConfigsType
{
    Match,
    NoMatch,
    Interval,
    Random
}
