using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinsConfigs : MonoBehaviour
{
	private static string instancePath = "SkinsConfigs";
	private static SkinsConfigs instance;

	public static SkinsConfigs Instance
	{
		get
		{
			if (!instance) instance = Resources.Load<SkinsConfigs>(instancePath);
			return instance;
		}
	}


	
}
