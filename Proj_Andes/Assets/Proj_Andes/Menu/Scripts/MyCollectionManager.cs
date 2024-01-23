using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCollectionManager : MonoBehaviour
{
    public static List<Monsters> totalDataCollection = new List<Monsters>();

    public Pool<MonsterItemUI> monstersUIInCollection;
    private void Awake()
    {
        Init();
    }
    // Start is called before the first frame update
    public void Init()
    {
        monstersUIInCollection.Init(10);

    }

    public void ShowItemsSaved()
    {
        for (int i = 0; i < totalDataCollection.Count; i++)
        {
            var item = monstersUIInCollection.GetNewItem();
            item.Show(totalDataCollection[i]);
        }
    }

}

