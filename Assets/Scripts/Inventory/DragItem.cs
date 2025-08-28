using UnityEngine;
using UnityEngine.EventSystems;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public InventorySlot parentSlot;
    private Canvas canvas;
    private RectTransform rectTransform;

    public GameObject currentItem;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        currentItem = gameObject;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentSlot = transform.parent.GetComponent<InventorySlot>();
        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.parent == canvas.transform)
        {
            SetParent(parentSlot);
        }
    }

    public void SetParent(InventorySlot slot)
    {
        parentSlot = slot;
        transform.SetParent(slot.transform);
        transform.localPosition = Vector3.zero;
    }
}
