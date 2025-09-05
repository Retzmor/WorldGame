using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public InventorySlot parentSlot;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private Transform originalParent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        if (transform.parent != null)
        {
            parentSlot = transform.parent.GetComponent<InventorySlot>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = transform.localPosition;
        originalParent = transform.parent;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;

        transform.SetParent(canvas.transform);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        InventorySlot targetSlot = FindSlotUnderCursor(eventData);

        if (targetSlot != null)
        {
            ExecuteEvents.Execute(targetSlot.gameObject, eventData, ExecuteEvents.dropHandler);
        }
        else
        {
            ReturnToOriginalSlot();
        }
    }

    private InventorySlot FindSlotUnderCursor(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            InventorySlot slot = result.gameObject.GetComponent<InventorySlot>();
            if (slot != null) return slot;

            Transform parent = result.gameObject.transform.parent;
            while (parent != null)
            {
                slot = parent.GetComponent<InventorySlot>();
                if (slot != null) return slot;
                parent = parent.parent;
            }
        }

        return null;
    }

    private void ReturnToOriginalSlot()
    {
        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;
    }

    public void SetParent(InventorySlot newSlot)
    {
        parentSlot = newSlot;
        transform.SetParent(newSlot.transform);
        transform.localPosition = Vector3.zero;
    }
}