using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_MagnetRangeHandler : MonoBehaviour
{

    [SerializeField] SpriteRenderer image;

    float timer = 0;

    public void Init(float radius)
    {
        image.transform.localScale = Vector3.one * radius;
        timer = 0;
        image.gameObject.SetActive(false);
    }

    public void ShowAt(Vector2 position)
    {
        transform.position = position;
        image.gameObject.SetActive(true);
        timer = 0;
    }

	private void Update()
	{
		if(image.gameObject.activeInHierarchy)
        {
            timer += Time.deltaTime;
            if(timer > 2)
            {
                timer = 0;
                image.gameObject.SetActive(false);
            }
        }
	}
}
