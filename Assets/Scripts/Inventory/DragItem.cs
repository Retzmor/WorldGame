using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class DragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Inject] Inventory inventory;
    [Inject] DiContainer container;
    [HideInInspector] public InventorySlot parentSlot;
    private Canvas canvas;
    public ItemData itemData;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector3 originalPosition;
    private Transform originalParent;
    public GameObject worldPrefab => itemData.worldPrefab;

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
            DropAllToWorld();
            Destroy(gameObject);
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
    public void SetParent(InventorySlot newSlot)
    {
        parentSlot = newSlot;
        transform.SetParent(newSlot.transform);
        transform.localPosition = Vector3.zero;
    }

    private void DropAllToWorld()
    {
        ItemUse itemData = GetComponent<ItemUse>();
        if (itemData == null) return;
        if (inventory == null) return;

        string itemName = itemData.itemName;
        if (!inventory.InventoryItems.ContainsKey(itemName)) return;

        int amount = inventory.InventoryItems[itemName];
        if (amount <= 0) return;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 dropOffset = Vector3.right; 
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null && sr.flipX) dropOffset = Vector3.left; 
        Vector3 dropPos = player.transform.position + dropOffset * 1f; 
        dropPos.z = 0f;
        GameObject dropped = container.InstantiatePrefab(itemData.worldPrefap, dropPos, Quaternion.identity, null);
        inventory.InventoryItems.Remove(itemName);
    }
}