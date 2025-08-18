using UnityEngine;

public class PotionItem : MonoBehaviour, IUsableItem
{
    public int healAmount = 40;
    public AudioClip drinkSfx;

    public void OnUseStart(PlayerItemUser user, HandSide hand)
    {
        user.stats?.Heal(healAmount); // clamps to max, but still consumes
        if (drinkSfx) AudioSource.PlayClipAtPoint(drinkSfx, user.transform.position);
        user.inventory.ConsumeFromHand(user.equipment, hand);
    }

    public void OnUseEnd(PlayerItemUser user, HandSide hand) { }
}
