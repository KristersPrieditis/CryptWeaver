// HotbarSlotUI.cs
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HotbarSlotUI : MonoBehaviour,
    IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    MortisHotbarUI ui;
    Image img;
    int slotIndex;

    public void Init(MortisHotbarUI ui, int slotIndex, Image img)
    {
        this.ui = ui;
        this.slotIndex = slotIndex;
        this.img = img;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Single-click to equip this slot (keeps left/right mapping by index)
        ui.ClickEquip(slotIndex);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!ui.HasItem(slotIndex)) return; // don't start drag from empty
        ui.BeginDrag(slotIndex, img ? img.sprite : null);
        ui.DragTo(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        ui.DragTo(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ui.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        ui.DropOn(slotIndex);
    }
}
