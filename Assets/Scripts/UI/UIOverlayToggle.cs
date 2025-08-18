// UIOverlayToggle.cs
using UnityEngine;

public class UIOverlayToggle : MonoBehaviour
{
    [Header("Targets")]
    public CanvasGroup overlayGroup;       // MouseOverlay (optional)
    public MortisHotbarUI hotbar;          // to cancel drags on close (optional)
    public PlayerLook playerLook;          // disable look when overlay is open (optional)

    [Header("Behavior")]
    public KeyCode toggleKey = KeyCode.Tab;
    public bool holdToOpen = false;        // true = hold Tab, false = toggle
    public bool disableLookWhileOpen = true;

    public bool IsOpen { get; private set; }   // <-- exposes state

    void Start()
    {
        SetOpen(false, true);
    }

    void Update()
    {
        if (holdToOpen)
        {
            bool shouldOpen = Input.GetKey(toggleKey);
            if (shouldOpen != IsOpen) SetOpen(shouldOpen);
        }
        else
        {
            if (Input.GetKeyDown(toggleKey)) SetOpen(!IsOpen);
            if (IsOpen && Input.GetKeyDown(KeyCode.Escape)) SetOpen(false);
        }
    }

    public void SetOpen(bool value, bool instant = false)
    {
        IsOpen = value;

        // Visual-only overlay (don't block drops/clicks)
        if (overlayGroup)
        {
            overlayGroup.alpha = IsOpen ? 1f : 0f;
            overlayGroup.blocksRaycasts = false;   // important: do not eat pointer events
            overlayGroup.interactable   = false;
        }

        Cursor.lockState = IsOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible   = IsOpen;

        if (disableLookWhileOpen && playerLook)
            playerLook.enabled = !IsOpen;

        // Safety: cancel any active drag when closing
        if (!IsOpen && hotbar)
            hotbar.EndDrag();
    }
}
