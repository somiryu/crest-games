using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinnableImage : MonoBehaviour
{
    public bool assignOnAwake = false;

	[SerializeField] Image image;
	public SpriteRenderer sRenderer;
	[SerializeField] ImagePerSkinType[] imagePerSkinTypes;

    private void OnValidate()
    {
        if (image == null) TryGetComponent(out image);
        if (sRenderer == null) TryGetComponent(out sRenderer);
    }

	private void Awake()
	{
        if (assignOnAwake)
        {
            SkinManager.Instance.RegisterSkinImg(this);
            SetSkinType(SkinManager.Instance.GetCurrSkin());
        }
	}

	public void SetSkinType(SkinType skinType)
    {
		Sprite sprite = null;
        for (int i = 0; i < imagePerSkinTypes.Length; i++)
        {
            if (imagePerSkinTypes[i].skinType == skinType)
            {
                sprite = imagePerSkinTypes[i].sprite;
                break;
            }
        }
        if(image) image.sprite = sprite;
        if(sRenderer) sRenderer.sprite = sprite;
    }
}

[Serializable]
public struct ImagePerSkinType
{
    public SkinType skinType;
    public Sprite sprite;
}
