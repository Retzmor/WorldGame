using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public bool isUsed = false;

    public void OnDrop(PointerEventData eventData)
    {
        DragItem dragItem = eventData.pointerDrag.GetComponent<DragItem>();
        if (dragItem != null)
        {
            InventorySlot fromSlot = dragItem.parentSlot;

            if (isUsed)
            {
                dragItem.SetParent(fromSlot);
                fromSlot.SetItem(dragItem.currentItem);
                SetItem(dragItem.currentItem);
            }
            else
            {
                dragItem.SetParent(this);
                SetItem(dragItem.currentItem);
                fromSlot.ClearItem();
            }
        }
    }

    public void SetItem(GameObject item)
    {
        item.transform.SetParent(transform);
        item.transform.localPosition = Vector3.zero;
        item.name = item.name.Replace("(Clone)", "");
        isUsed = true;
    }

    public void ClearItem()
    {
        isUsed = false;
    }
}
