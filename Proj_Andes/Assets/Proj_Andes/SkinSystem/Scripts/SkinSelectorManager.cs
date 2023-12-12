using System.Collections;
using System.Collections.Generic;
using Tymski;
using UnityEngine;

[CreateAssetMenu(fileName = "SkinSelectorManager", menuName = "GameSequencesList/SkinSelectorManager")]
public class SkinSelectorItem : GameSequenceItem
{

}
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
