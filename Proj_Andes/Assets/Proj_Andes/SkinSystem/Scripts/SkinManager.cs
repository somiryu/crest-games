using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    private static SkinManager instance;
    public static SkinManager Instance => instance;

	private List<SkinnableImage> allSkinnableImages = new List<SkinnableImage>();

	private void Awake()
	{
		if(instance != null && instance != this) Destroy(instance);
		instance = this;

		Utility.FindObjectsByType(allSkinnableImages);
	}

	public void SetSkin(SkinType skinToSet)
	{
		for(int i = 0; i < allSkinnableImages.Count; i++)
		{
			allSkinnableImages[i].SetSkinType(skinToSet);
		}
	}


}

public enum SkinType
{
	Galactic,
	flowers,
	Hearths
}
