using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DialoguesConfigs", menuName = "DialogueSystem/Configs")]
public class DialogueConfigs : ScriptableObject {

    private static string instancePath = "DialoguesConfigs";
    private static DialogueConfigs instance;

    public static DialogueConfigs Instace {
        get {
            if (!instance) instance = Resources.Load<DialogueConfigs>(instancePath);
            return instance;
        }
    }
    public float appearTime;
    public KeyCode skipKey;

    public CharacterUIConfig[] charactersUIConfigs;

    public CharacterUIConfig GetDialogConfigFor(CharactersTypes chType) {
        for (int i = 0; i < charactersUIConfigs.Length; i++) {
            var curr = charactersUIConfigs[i];
            if(curr.chType == chType) {
                return curr;
            }
        }
        Debug.LogError("Asking for character configs of type: " + chType + " But no configs were found");
        return null;
    }

}

public enum CharactersTypes {
    NONE = 0,
    Cami = 1,
    Tomas = 2,
    Alex = 3,
    Profe = 4,
    Narrador = 5,
}

[Serializable]
public class CharacterUIConfig {
    public CharactersTypes chType;
    public Sprite image;
    public string name;
}
