using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SkinnableObject : MonoBehaviour
{
    [SerializeField] ChangeableItem[] items;
    public Action<Transform> OnCurrSkinObjChanged;
    [NonSerialized] public Transform currSkinObj;
    [SerializeField] bool assignOnAwake;

    private void Awake()
    {
        if (!assignOnAwake) return;
        SwitchItem(SkinManager.Instance.GetCurrSkin(), out var Item);
        SkinManager.Instance.RegisterSkinObj(this);
            
    }
    public void SwitchItem(SkinType skinType, out Transform currObject)
    {
        var previousItem = currSkinObj;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].skinType == skinType)
            {
                items[i].item.gameObject.SetActive(true);
                currSkinObj = items[i].item;
            }
            else items[i].item.gameObject.SetActive(false);
        }
        currObject = currSkinObj;
        if (previousItem != currSkinObj) OnCurrSkinObjChanged?.Invoke(currSkinObj);
    }
}
[Serializable]
public struct ChangeableItem
{
    public SkinType skinType;
    public Transform item;
}