using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyCollectionManager : MonoBehaviour
{
    public Transform collectionSet;
    [SerializeField] Button myCollBtn;
    [SerializeField] Button hideCollectionBtn;

    public static List<Monsters> totalDataCollection = new List<Monsters>();
    public Pool<MonsterItemUI> monstersUIInCollection;
    private void Awake()
    {
        Init();
    }
    public void Init()
    {
        monstersUIInCollection.Init(10);

        myCollBtn.onClick.AddListener(ShowCollection);
        hideCollectionBtn.onClick.AddListener(HideCollection);

        HideCollection();
    }

    public void ShowItemsSaved()
    {
        Debug.Log(totalDataCollection.Count);
        monstersUIInCollection.RecycleAll();
        for (int i = 0; i < totalDataCollection.Count; i++)
        {
            var item = monstersUIInCollection.GetNewItem();
            item.Show(totalDataCollection[i]);
        }
    }

    public void ShowCollection()
    {
        collectionSet.gameObject.SetActive(true);
        myCollBtn.gameObject.SetActive(false);
        ShowItemsSaved();
    }
    public void HideCollection()
    {
        myCollBtn.gameObject.SetActive(true);
        monstersUIInCollection.RecycleAll();
        collectionSet.gameObject.SetActive(false);
    }

}

