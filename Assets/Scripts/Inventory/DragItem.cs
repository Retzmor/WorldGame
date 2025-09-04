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

        // Guarda el slot inicial (padre del item)
        parentSlot = transform.parent.GetComponent<InventorySlot>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Guarda el slot original
        parentSlot = transform.parent.GetComponent<InventorySlot>();

        // Lo sacamos temporalmente al canvas para que no se "recorte"
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
            // Vuelve al slot original si no hay destino
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
    }
}
