using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyCollectionManager : MonoBehaviour
{
    static MyCollectionManager instance;
    public static MyCollectionManager Instance => instance;
    public Transform collectionSet;
    [SerializeField] Button myCollBtn;
    [SerializeField] Button hideCollectionBtn;
    [SerializeField] Image leftArrow;
    [SerializeField] Image rightArrow;
    [HideInInspector] public bool toTheRight;
    public ScrollRect scrollbar;
    [HideInInspector] public float scrollSpeed = 2f;

    public static List<Monsters> totalDataCollection = new List<Monsters>();
    public Pool<MonsterItemUI> monstersUIInCollection;

    bool useShowCollectionBtn;


    private void Awake()
    {
        if (instance != null && instance != this) DestroyImmediate(this);
        instance = this;
    }
    public void Init(bool inUseShowCollectionBtn)
    {
        useShowCollectionBtn = inUseShowCollectionBtn;
        monstersUIInCollection.Init(10);
        scrollSpeed = 0;
        myCollBtn.onClick.AddListener(ShowCollection);
        myCollBtn.gameObject.SetActive(useShowCollectionBtn);
        hideCollectionBtn.onClick.AddListener(HideCollection);

        HideCollection();
    }
    private void Update()
    {
        scrollbar.horizontalNormalizedPosition += scrollSpeed * Time.deltaTime;
    }
    public void ShowItemsSaved()
    {
        Debug.Log(totalDataCollection.Count);
        monstersUIInCollection.RecycleAll();
        for (int i = 0; i < totalDataCollection.Count; i++)
        {
            var item = monstersUIInCollection.GetNewItem();
            item.Show(totalDataCollection[i], true);
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
    public void OnScroll()
    {
        if (toTheRight) scrollSpeed = 2;
        else scrollSpeed = -2;
        Debug.Log("moving");
    }
    public void OnStopScrolling()
    {
        scrollSpeed = 0;
    }
}

