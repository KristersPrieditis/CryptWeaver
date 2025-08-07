using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HotbarController : MonoBehaviour
{
    [SerializeField] private List<Image> slotIcons;

    public void SetSlotIcon(int index, Sprite icon)
    {
        if (index >= 0 && index < slotIcons.Count)
        {
            slotIcons[index].sprite = icon;
            slotIcons[index].enabled = icon != null;
        }
    }

    public void ClearSlot(int index)
    {
        if (index >= 0 && index < slotIcons.Count)
        {
            slotIcons[index].sprite = null;
            slotIcons[index].enabled = false;
        }
    }
}