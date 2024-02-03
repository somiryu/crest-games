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

    bool useShowCollectionBtn;


	public void Init(bool inUseShowCollectionBtn)
    {
        useShowCollectionBtn = inUseShowCollectionBtn;
        monstersUIInCollection.Init(10);

        myCollBtn.onClick.AddListener(ShowCollection);
        myCollBtn.gameObject.SetActive(useShowCollectionBtn);
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
        myCollBtn.gameObject.SetActive(useShowCollectionBtn);
        monstersUIInCollection.RecycleAll();
        collectionSet.gameObject.SetActive(false);
    }

}

