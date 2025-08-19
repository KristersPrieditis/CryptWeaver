using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorSceneLoader : MonoBehaviour
{
    [Header("Requirement")]
    public bool requireKey = true;
    public string requiredItemId;      // e.g. "key.firstfloor.01"
    public string requiredItemName;    // fallback, e.g. "Dungeon Key"
    public bool consumeKey = true;

    [Header("Next Scene")]
    public string nextSceneName = "SafeRoom_01";
    public string nextSceneSpawnId = "FireSpawn";
    public float delayBeforeLoad = -1f; // -1 uses openSfx length

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSfx;

    public void TryOpen(PlayerInventory inv, PlayerEquipment eq)
    {
        if (!inv) return;

        if (requireKey)
        {
            int idx = FindKeyIndex(inv);
            if (idx == -1) return; // locked

            if (consumeKey && idx >= 0)
            {
                if (inv.equippedLeft == idx)  { eq?.Unequip(HandSide.Left);  inv.equippedLeft  = -1; }
                if (inv.equippedRight == idx) { eq?.Unequip(HandSide.Right); inv.equippedRight = -1; }
                inv.slots[idx] = null;
            }
        }

        float wait = 0f;
        if (openSfx)
        {
            if (audioSource) audioSource.PlayOneShot(openSfx);
            else AudioSource.PlayClipAtPoint(openSfx, transform.position);
            wait = (delayBeforeLoad >= 0f) ? delayBeforeLoad : openSfx.length;
        }
        else if (delayBeforeLoad >= 0f) wait = delayBeforeLoad;

        // set spawn for next scene and autosave the transition
        SceneSpawnRouter.SetNext(nextSceneSpawnId);
        SaveManager.SaveProgress(nextSceneName, nextSceneSpawnId,
            inv.transform, inv.GetComponent<PlayerStats>(), inv);

        StartCoroutine(LoadAfter(wait));
    }

    int FindKeyIndex(PlayerInventory inv)
    {
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
        if (seconds > 0f) yield return new WaitForSeconds(seconds);
        SceneManager.LoadScene(nextSceneName);
    }
}