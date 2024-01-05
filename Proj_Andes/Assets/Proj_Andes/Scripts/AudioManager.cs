using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class AudioManager
{
    public static AudioSource currentAudio;
    public static void PlayAudio()
    {
        currentAudio.Play();
    }
    public static void PauseAudio()
    {
        currentAudio.Pause();
    }
}
