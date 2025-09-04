using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        parentSlot = transform.parent.GetComponent<InventorySlot>();
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
        InventorySlot newSlot = null;

        if (eventData.pointerEnter != null)
        {
            newSlot = eventData.pointerEnter.GetComponent<InventorySlot>();
        }

        if (newSlot != null)
        {
            SetParent(newSlot);
        }
        else if (parentSlot != null)
        {
            SetParent(parentSlot);
        }
        else
        {
            Debug.LogWarning("Este item no tiene un slot asignado.");
        }
    }

    public void SetParent(InventorySlot slot)
    {
        parentSlot = slot;
        transform.SetParent(slot.transform);
        transform.localPosition = Vector3.zero;
        slot.isUsed = true;
    }
}