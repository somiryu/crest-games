using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class ScrollBehavior : Image , IPointerDownHandler
{
    MyCollectionManager myColl => MyCollectionManager.Instance;

    public void OnPointerDown(PointerEventData eventData)
    {
        myColl.OnScroll();
    }

}
