using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AppSaveSystem : MonoBehaviour
{
	private static string instancePrefabPath = "AppSaveSystem";

	private static AppSaveSystem instance;
	public static AppSaveSystem Instance => instance;

	[RuntimeInitializeOnLoadMethod]
	private static void RunOnStart()
	{
		if(instance == null)
		{
			var instancePrefab = Resources.Load<AppSaveSystem>(instancePrefabPath);
			instance = GameObject.Instantiate(instancePrefab);
		}
	}


	private void Awake()
	{
		if(instance != null && instance != this) DestroyImmediate(instance);
		instance = this;
		Object.DontDestroyOnLoad(this);
	}
}
