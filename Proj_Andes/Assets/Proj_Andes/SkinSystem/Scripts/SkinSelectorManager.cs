using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;

public class SkinSelectorManager : MonoBehaviour
{
    static SkinSelectorManager instance;
    public static SkinSelectorManager Instance => instance;
    public static SceneReference skinSelectionScene;
    SkinType currSkinType;
    private void Awake()
    {
        if (instance)
        {
            if (instance != this) DestroyImmediate(this);
        }
        instance = this;
    }

}
