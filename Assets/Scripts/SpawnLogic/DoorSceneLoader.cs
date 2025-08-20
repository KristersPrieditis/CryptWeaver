using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorSceneLoader : MonoBehaviour
{
    [Header("Requirement")]
    public bool requireKey = true;
    public string requiredItemId;      // preferred: stable itemId (e.g. "key.firstfloor.01")
    public string requiredItemName;    // fallback: display name if id isn't set
    public bool consumeKey = true;     // true = remove the key from inventory when used

    [Header("Next Scene")]
    public string nextSceneName = "SafeRoom_01";
    public string nextSceneSpawnId = "FireSpawn";
    public float delayBeforeLoad = -1f; // -1 = use openSfx length, else use this exact delay

    [Header("Audio")]
    public AudioSource audioSource;    // optional, will fall back to PlayClipAtPoint
    public AudioClip openSfx;          // optional

    public void TryOpen(PlayerInventory inv, PlayerEquipment eq)
    {
        if (!inv) return; // nothing to check against

        // Gate: needs a key? then find it
        if (requireKey)
        {
            int idx = FindKeyIndex(inv);
            if (idx == -1) return; // locked: no matching key, do nothing

            // Optionally consume the key and clean up any equips pointing at that slot
            if (consumeKey && idx >= 0)
            {
                if (inv.equippedLeft == idx)  { eq?.Unequip(HandSide.Left);  inv.equippedLeft  = -1; }
                if (inv.equippedRight == idx) { eq?.Unequip(HandSide.Right); inv.equippedRight = -1; }
                inv.slots[idx] = null;
            }
        }

        // Decide how long to wait (SFX length or explicit delay)
        float wait = 0f;
        if (openSfx)
        {
            if (audioSource) audioSource.PlayOneShot(openSfx);
            else AudioSource.PlayClipAtPoint(openSfx, transform.position);
            wait = (delayBeforeLoad >= 0f) ? delayBeforeLoad : openSfx.length;
        }
        else if (delayBeforeLoad >= 0f) wait = delayBeforeLoad;

        // Hand off spawn for the next scene + autosave this transition
        SceneSpawnRouter.SetNext(nextSceneSpawnId);
        SaveManager.SaveProgress(nextSceneName, nextSceneSpawnId,
            inv.transform, inv.GetComponent<PlayerStats>(), inv);

        // Load after the small delay so the audio has time to fire
        StartCoroutine(LoadAfter(wait));
    }

    int FindKeyIndex(PlayerInventory inv)
    {
        // Look for a match by itemId first, then by name as a fallback
        for (int i = 0; i < inv.slots.Length; i++)
        {
            var d = inv.slots[i];
            if (!d) continue;
            if (!string.IsNullOrEmpty(requiredItemId) && d.itemId == requiredItemId) return i;
            if (!string.IsNullOrEmpty(requiredItemName) && d.itemName == requiredItemName) return i;
        }
        return -1;
    }

    IEnumerator LoadAfter(float seconds)
    {
        if (seconds > 0f) yield return new WaitForSeconds(seconds); // give SFX a moment
        SceneManager.LoadScene(nextSceneName);
    }
}
