using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    [Header("Slot Settings")]
    public bool isHotbarSlot = false;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;
        if (droppedObject == null) return;

        DragItem dragItem = droppedObject.GetComponent<DragItem>();
        if (dragItem == null) return;

        InventorySlot fromSlot = dragItem.parentSlot;
        InventorySlot toSlot = this;

        if (fromSlot == toSlot) return;

        bool slotHasItem = transform.childCount > 0;

        if (slotHasItem)
        {
            Transform existingChild = transform.GetChild(0);
            DragItem existingItem = existingChild.GetComponent<DragItem>();

            if (existingItem != null)
            {
                existingItem.transform.SetParent(fromSlot.transform);
                existingItem.transform.localPosition = Vector3.zero;
                existingItem.parentSlot = fromSlot;
            }
        }
        else
        {
            fromSlot.ClearSlotVisual();
        }

        dragItem.SetParent(toSlot);
    }

    public void ClearSlotVisual()
    {
        // Método para limpieza visual si es necesario
    }

    public bool IsEmpty()
    {
        return transform.childCount == 0;
    }
}