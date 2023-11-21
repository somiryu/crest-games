using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue Sequence Data", menuName = "DialogueSystem/Sequence")]
public class DialogueSequenceData : ScriptableObject {
    public DialogueData[] dialogues;

    public override string ToString()
    {
        if (dialogues != null && dialogues.Length > 0) {
            DialogueData firstLine = dialogues[0];
            return $"{firstLine.characterType}: {firstLine.text}";
        }
        return name;
    }
}

[Serializable]
public class DialogueData
{
    public CharactersTypes characterType;
    [TextArea(1, 20)]
    public string text;
    public DialogueResponse[] responses;
}


[Serializable]
public class DialogueResponse
{
    [TextArea(1, 20)]
    public string response;
    public bool changeSequence;
    public DialogueSequenceData dataAfterResponse;
}
