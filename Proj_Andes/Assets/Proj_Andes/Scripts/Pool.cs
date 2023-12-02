using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Pool <T> where T : Component
{
	private List<T> allItems;
	private List<bool> itemsAvailability;
	[SerializeField]
	private T itemPrefab;
	public Transform hierarchyParent;
	[SerializeField]
	private int growBy;

    public void Init(int initialSize, bool activeInitialObjs = false)
	{
		allItems = new List<T>(initialSize);
		itemsAvailability = new List<bool>(initialSize);
		GrowPool(initialSize);
		for (int i = 0; i < allItems.Count; i++)
		{
			allItems[i].gameObject.SetActive(activeInitialObjs);
		}
	}

	public void GrowPool(int newItemsAmount)
	{
		for (int i = 0; i < newItemsAmount; i++)
		{
			var newItem = GameObject.Instantiate(itemPrefab, hierarchyParent);
			allItems.Add(newItem);
			itemsAvailability.Add(true);
			newItem.gameObject.SetActive(false);
		}
	}

	public T GetNewItem(bool activeObj = true)
	{
		T itemFound = null;
		for (int i = 0; i < itemsAvailability.Count; i++)
		{
			if (!itemsAvailability[i]) continue;
			itemsAvailability[i] = false;
			itemFound = allItems[i];
			break;
		}
		if (itemFound == null)
		{
			var lastCount = itemsAvailability.Count;
			//Grow
			GrowPool(growBy);
			itemsAvailability[lastCount] = false;
			itemFound = allItems[lastCount];
		}
		if (activeObj)
		{
			itemFound.gameObject.SetActive(true);
		}
		return itemFound;
	}

	/// <summary>
	/// Returns all the objs that are not on the pool right now
	/// </summary>
	public List<T> GetObjsBeingUsed()
	{
		List<T> objs = new List<T>();
		for (int i = 0; i < allItems.Count; i++)
		{
			if (itemsAvailability[i]) continue;
			objs.Add(allItems[i]);
		}
		return objs;
	}

	public void RecycleItem(T item)
	{
        if (item == null)
        {
			Debug.LogError("Trying to recycle a null obj");
        }
        var idx = allItems.IndexOf(item);
		item.gameObject.SetActive(false);
		if(idx >= itemsAvailability.Count || idx < 0)
		{
			Debug.Log("Trying to recycle obj but index out of bound: " + idx, item);
		}
		itemsAvailability[idx] = true;
	}

	public void RecycleAll(bool deactiveButtons = true)
	{
		for (int i = 0; i < allItems.Count; i++)
		{
			if (deactiveButtons)
			{
				allItems[i].gameObject.SetActive(false);
			}
			itemsAvailability[i] = true;
		}
	}

}
