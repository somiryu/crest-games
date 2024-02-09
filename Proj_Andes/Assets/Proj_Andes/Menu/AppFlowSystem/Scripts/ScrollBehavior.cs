using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class ScrollBehavior : Image , IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] bool toTheRight;
    [SerializeField] ScrollRect scrollbar;
    [SerializeField] float scrollSpeed;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (toTheRight) scrollbar.horizontalNormalizedPosition -= scrollSpeed;
        else scrollbar.horizontalNormalizedPosition += scrollSpeed;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

}
