// MortisHotbarUI.cs
using UnityEngine;
using UnityEngine.UI;

public class MortisHotbarUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerInventory inventory;
    public PlayerEquipment equipment;

    [Header("Slots (Images you see in the UI)")]
    public Image[] leftSlots;   // size 3 (indices 0..2)
    public Image[] rightSlots;  // size 3 (indices 3..5)

    [Header("Drag visuals")]
    public Canvas canvas;             // your UI canvas
    public RectTransform dragLayer;   // full-screen RT under the canvas
    public Image dragGhostPrefab;     // optional; auto-created if null

    [Header("Tints")]
    public Color emptyTint  = new Color(1, 1, 1, 0.2f);
    public Color filledTint = Color.white;
    public Color equippedTint = new Color(1, 1, 1, 0.95f);

    Image dragGhost;
    int draggingFrom = -1;

    void Awake()
    {
        // Attach HotbarSlotUI to each slot & index them
        for (int i = 0; i < leftSlots.Length; i++) HookSlot(leftSlots[i], i);
        for (int i = 0; i < rightSlots.Length; i++) HookSlot(rightSlots[i], i + 3);

        // Create a drag ghost if none provided
        if (!dragGhostPrefab && dragLayer)
        {
            var go = new GameObject("DragGhost", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(dragLayer, false);
            dragGhost = go.GetComponent<Image>();
        }
        else if (dragGhostPrefab && dragLayer)
        {
            dragGhost = Instantiate(dragGhostPrefab, dragLayer);
        }
        if (dragGhost)
        {
            dragGhost.raycastTarget = false;
            dragGhost.enabled = false;
        }
    }

    void Update() => Refresh();

    void HookSlot(Image img, int slotIndex)
    {
        if (!img) return;
        var slot = img.GetComponent<HotbarSlotUI>();
        if (!slot) slot = img.gameObject.AddComponent<HotbarSlotUI>();
        slot.Init(this, slotIndex, img);
    }

    public void Refresh()
    {
        // Left 0–2
        for (int i = 0; i < 3; i++)
            PaintSlot(leftSlots[i], inventory.slots[i], i, inventory.equippedLeft);

        // Right 3–5
        for (int i = 0; i < 3; i++)
            PaintSlot(rightSlots[i], inventory.slots[i + 3], i + 3, inventory.equippedRight);
    }

    void PaintSlot(Image img, ItemData data, int slotIndex, int equippedIndexForSide)
    {
        if (!img) return;
        if (data && data.icon)
        {
            img.sprite = data.icon;
            img.color  = (slotIndex == equippedIndexForSide) ? equippedTint : filledTint;
            img.enabled = true;
        }
        else
        {
            img.sprite = null;
            img.color  = emptyTint;
            img.enabled = true;
        }
    }

    // ===== Called by HotbarSlotUI =====
    public bool HasItem(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < inventory.slots.Length &&
               inventory.slots[slotIndex] != null;
    }

    public void BeginDrag(int fromIndex, Sprite sprite)
    {
        draggingFrom = fromIndex;
        if (dragGhost)
        {
            dragGhost.sprite = sprite;
            dragGhost.enabled = sprite != null;
        }
    }

    public void DragTo(Vector2 screenPos)
    {
        if (!dragGhost || !dragGhost.enabled || !canvas || !dragLayer) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragLayer, screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out var local);
        dragGhost.rectTransform.anchoredPosition = local;
    }

    public void DropOn(int toIndex)
    {
        if (draggingFrom == -1) { EndDrag(); return; }
        if (toIndex < 0 || toIndex >= inventory.slots.Length) { EndDrag(); return; }
        if (draggingFrom == toIndex) { EndDrag(); return; }

        // Swap items
        int a = draggingFrom, b = toIndex;
        var tmp = inventory.slots[a];
        inventory.slots[a] = inventory.slots[b];
        inventory.slots[b] = tmp;

        // Keep equipped indices correct if they referenced swapped slots
        if (inventory.equippedLeft == a) inventory.equippedLeft = b;
        else if (inventory.equippedLeft == b) inventory.equippedLeft = a;

        if (inventory.equippedRight == a) inventory.equippedRight = b;
        else if (inventory.equippedRight == b) inventory.equippedRight = a;

        EndDrag();
        Refresh();
    }

    public void EndDrag()
    {
        draggingFrom = -1;
        if (dragGhost) dragGhost.enabled = false;
    }

    public void ClickEquip(int slotIndex)
    {
        inventory.EquipSlot(slotIndex, equipment);
    }
}

