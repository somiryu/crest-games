using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkinManager : MonoBehaviour
{
    private static SkinManager instance;
    public static SkinManager Instance => instance;

	List<SkinnableImage> allSkinnableImages = new List<SkinnableImage>();
	List<SkinnableImageRandom> allSkinnableImagesRandom = new List<SkinnableImageRandom>();
	List<SkinnableObject> allItems = new List<SkinnableObject>();
	public Transform currentSkinObject;

	public bool forceSkinType = false;
	[SerializeField] Transform blockingPanel;
	[SerializeField] AudioClip pickASkinAudio;
	AudioSource source;
	public SkinType skinTypeToForce;

	private void Awake()
	{
		if(instance != null && instance != this) Destroy(instance);
		instance = this;
		TryGetComponent(out source);
		Utility.FindObjectsByType(allSkinnableImages);
		Utility.FindObjectsByType(allSkinnableImagesRandom);
		Utility.FindObjectsByType(allItems);
	}
	IEnumerator Instructions()
	{
		blockingPanel.gameObject.SetActive(true);
		source.clip = pickASkinAudio;
		source.Play();
		yield return new WaitForSeconds(pickASkinAudio.length);
        blockingPanel.gameObject.SetActive(false);
    }
    private void Start()
	{
		StartCoroutine(Instructions());
		if (forceSkinType) SetSkin(skinTypeToForce);
		else SetSkin(SceneManagement.currSkinType);
	}
	public void SetSkin(SkinType skinToSet)
	{
		for(int i = 0; i < allSkinnableImages.Count; i++)
		{
			allSkinnableImages[i].SetSkinType(skinToSet);
        }
		for (int i = 0; i < allItems.Count ; i++)
		{
            allItems[i].SwitchItem(skinToSet, out currentSkinObject);
        }
		for (int i = 0; i < allSkinnableImagesRandom.Count; i++)
		{
			allSkinnableImagesRandom[i].SetSkinType(skinToSet);
		}
	}

	public SkinType GetCurrSkin()
	{
		if (forceSkinType) return skinTypeToForce;
		else return SceneManagement.currSkinType;
	}

	public void RegisterSkinImg(SkinnableImage img)
	{
		if (allSkinnableImages.Contains(img)) return;
		allSkinnableImages.Add(img);
	}
	public void RegisterSkinRandomImg(SkinnableImageRandom img)
	{
		if (allSkinnableImagesRandom.Contains(img)) return;
		allSkinnableImagesRandom.Add(img);
	}

	public void RegisterSkinObj(SkinnableObject obj)
	{
		if (allItems.Contains(obj)) return;
		allItems.Add(obj);
	}

}

public enum SkinType
{
	Galactic,
	Aquatic,
	Magic,
}
