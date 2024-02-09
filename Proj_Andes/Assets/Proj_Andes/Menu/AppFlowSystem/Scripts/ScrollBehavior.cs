using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class ScrollBehavior : Image , IPointerDownHandler, IPointerUpHandler
{
    MyCollectionManager myColl => MyCollectionManager.Instance;

    public void OnPointerDown(PointerEventData eventData)
    {
        myColl.OnScroll();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        myColl.OnStopScrolling();
    }
}
