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
    public GameObject worldInstance;
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

        bool pointerOverUI = EventSystem.current.IsPointerOverGameObject();

        if (pointerOverUI)
        {
            InventorySlot targetSlot = FindSlotUnderCursor(eventData);
            if (targetSlot != null)
            {
                ExecuteEvents.Execute(targetSlot.gameObject, eventData, ExecuteEvents.dropHandler);
                return;
            }
        }

        // Si no está sobre UI o slot, soltar al mundo
        Debug.Log("Soltado fuera del inventario, soltando al mundo...");
        DropAllToWorld(itemData);
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

    private void DropAllToWorld(ItemData itemData)
    {
        if (itemData == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        AttackPlayer attackPlayer = player.GetComponent<AttackPlayer>();
        GameObject droppedItem;

        if (attackPlayer != null &&
             attackPlayer.currentWeapon != null &&
            attackPlayer.currentWeaponData != null &&
            attackPlayer.currentWeaponData.itemName == itemData.itemName)
        {
            Debug.Log("Quite el arma");
            droppedItem = attackPlayer.currentWeapon;
            attackPlayer.currentWeapon = null;
            attackPlayer.currentWeaponData = null;
            attackPlayer.currentWeaponType = null;

            droppedItem.transform.SetParent(null);
        }
        else
        {
            if (itemData.worldPrefab == null) return;
            droppedItem = container.InstantiatePrefab(itemData.worldPrefab);
        }

        Vector3 dropOffset = player.transform.right;
        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null && sr.flipX)
            dropOffset = -player.transform.right;

        Vector3 worldPos = player.transform.position + dropOffset * 1f;
        droppedItem.transform.position = worldPos;
        droppedItem.transform.rotation = Quaternion.identity;

        Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
        if (rb == null) rb = droppedItem.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.linearDamping = 2f;
        rb.angularDamping = 1f;
        rb.linearVelocity = dropOffset * 3f;

        if (droppedItem.GetComponent<Collider2D>() == null)
            droppedItem.AddComponent<BoxCollider2D>();

        Destroy(gameObject);
    }

}
