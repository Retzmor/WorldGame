using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public bool isUsed = false;

    public void OnDrop(PointerEventData eventData)
    {
        DragItem draggedItem = eventData.pointerDrag.GetComponent<DragItem>();
        if (draggedItem == null) return;

        InventorySlot fromSlot = draggedItem.parentSlot;

        if (transform.childCount > 0)
        {
            Transform existingItem = transform.GetChild(0);
            existingItem.SetParent(fromSlot.transform);
            existingItem.localPosition = Vector3.zero;
            fromSlot.isUsed = true;
        }
        else
        {
            fromSlot.isUsed = false;
        }

        draggedItem.SetParent(this);
        isUsed = true;
    }
}
