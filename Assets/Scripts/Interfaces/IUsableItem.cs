using UnityEngine;

public interface IUsableItem
{
    // Called by player when starting/holding/releasing use
    void OnUseStart(PlayerItemUser user, HandSide hand);
    void OnUseEnd(PlayerItemUser user, HandSide hand);
}
