using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelectorUI : MonoBehaviour
{
    public List<BtnPerSkinType> btnsPerSkins = new List<BtnPerSkinType>();

	private void Awake()
	{
		for (int i = 0; i < btnsPerSkins.Count; i++)
		{
			var curr = btnsPerSkins[i];
			curr.btn.onClick.AddListener(() => SkinManager.Instance.SetSkin(curr.skinType));
		}
	}

}


[Serializable]
public struct BtnPerSkinType
{
    public SkinType skinType;
    public Button btn;
}
