using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public static class Utility 
{
    public static void FindObjectsByType<T>(List<T> objectsTList)
    {
        var rootObjs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootObjs.Length; i++)
        {
            var curr = rootObjs[i];
            GetObjectsOfType<T>(curr, objectsTList);
        }
    }

    static void GetObjectsOfType<T>(GameObject currObj, List<T> objectsTList)
    {
        if (currObj.TryGetComponent(out T obj))
        {
            objectsTList.Add(obj);
        }
        for (int i = 0; i < currObj.transform.childCount; i++)
        {
            var currChild = currObj.transform.GetChild(i);
            GetObjectsOfType(currChild.gameObject, objectsTList);
        }
    }

    public static T FindObjectByType<T>() where T : Component
    {
        var rootObjs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootObjs.Length; i++)
        {
            var curr = rootObjs[i];
            T found = GetObjectOfType<T>(curr);
            if (found != null) return found;
        }
        return null;
    }

    static T GetObjectOfType<T>(GameObject currObj) where T : Component
    {
        if (currObj.TryGetComponent(out T obj))
        {
            return obj;
        }
        for (int i = 0; i < currObj.transform.childCount; i++)
        {
            var currChild = currObj.transform.GetChild(i);
            var found = GetObjectOfType<T>(currChild.gameObject);
            if (found) return found;
        }
        return null;
    }

    public static CharacterUIConfig GetCharacterConfig(this CharactersTypes ofType)
    {
        var dialogsConfigs = DialogueConfigs.Instace;
        if (!dialogsConfigs)
        {
            Debug.LogError("Dialogs configs not found");
            return null;
        }
        return dialogsConfigs.GetDialogConfigFor(ofType);
    }
    public static bool IsInLayerMask(this LayerMask layerMask, int objLayerMask)
    {
        if ((layerMask.value & (1 << objLayerMask)) > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool Evaluate(this Comparison comparison, float v1, float v2)
	{
		switch (comparison)
		{
			case Comparison.Equals:
                return v1 == v2;
			case Comparison.MoreThan:
                return v1 > v2;
			case Comparison.LessThan:
                return v1 < v2;
			case Comparison.MoreOrEquals:
                return v1 >= v2;
			case Comparison.LessOrEquals:
                return v1 <= v2;
		}
        return false;
	}

    public static int GetAnalyticValue(this FrustrationLevel level)
    {
        return level switch
        {
            FrustrationLevel.NONE => 0,
            FrustrationLevel.Muy_Tranquilo => 1,
            FrustrationLevel.Un_Poco_Tranquilo => 2,
            FrustrationLevel.Un_Poco_Frustrado => 3,
            FrustrationLevel.Muy_Frustrado => 4,
            _ => -1
        };
    }
}

public enum Comparison
{
    Equals,
    MoreThan,
    LessThan,
    MoreOrEquals,
    LessOrEquals,
}
