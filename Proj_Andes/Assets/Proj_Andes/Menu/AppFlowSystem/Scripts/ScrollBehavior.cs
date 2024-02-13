using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class ScrollBehavior : MonoBehaviour , IPointerDownHandler, IPointerUpHandler
{
    MyCollectionManager myColl => MyCollectionManager.Instance;
    public bool toTheRight = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        myColl.OnScroll(toTheRight);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        myColl.OnStopScrolling();
    }
}
