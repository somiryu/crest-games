using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SkinnableObject : MonoBehaviour
{
    [SerializeField] ChangeableItem[] items;
    public Action<Transform> OnCurrSkinObjChanged;
    [SerializeField] Transform item;
    void Awake()
    {
        for (int i = 0; i < items.Length; i++) items[i].item.gameObject.SetActive(false);
        item = items[0].item;
    }
    public void SwitchItem(SkinType skinType, out Transform currObject)
    {
        var previousItem = item;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].skinType == skinType)
            {
                items[i].item.gameObject.SetActive(true);
                item = items[i].item;
            }
            else items[i].item.gameObject.SetActive(false);
        }
        currObject = item;
        if (previousItem != item) OnCurrSkinObjChanged?.Invoke(item);
    }
}
[Serializable]
public struct ChangeableItem
{
    public SkinType skinType;
    public Transform item;
}