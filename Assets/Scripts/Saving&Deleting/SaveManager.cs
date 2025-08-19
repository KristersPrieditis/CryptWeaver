using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using SQLite;

public static class SaveManager
{
    // POCO row mapped by sqlite-net
    public class SaveRow
    {
        [PrimaryKey] public string Id { get; set; }          // e.g., "Autosave"
        public long   Time { get; set; }                     // unix seconds
        public string Scene { get; set; }                    // scene name
        public string SpawnId { get; set; }                  // spawn id in that scene
        public float  Px { get; set; }                       // (optional) position
        public float  Py { get; set; }
        public float  Pz { get; set; }
        public int    Health { get; set; }
        public string Inventory { get; set; }                // pipe-delimited itemIds
        public int    EquippedLeft { get; set; }
        public int    EquippedRight { get; set; }
    }

    static string DbFile => Path.Combine(Application.persistentDataPath, "mortis.db3");

    static SQLiteConnection _conn;
    static SQLiteConnection Conn
    {
        get
        {
            if (_conn == null)
            {
                // Create DB if missing; SharedCache helps on some platforms
                _conn = new SQLiteConnection(
                    DbFile,
                    SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache
                );
                _conn.CreateTable<SaveRow>();
            }
            return _conn;
        }
    }

    public static void SaveProgress(string sceneName, string spawnId,
                                    Transform player, PlayerStats stats, PlayerInventory inv)
    {
        if (!player || !stats || !inv) return;

        var row = new SaveRow
        {
            Id = "Autosave",
            Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Scene = sceneName,
            SpawnId = spawnId ?? "",
            Px = player.position.x,
            Py = player.position.y,
            Pz = player.position.z,
            Health = stats.CurrentHealth,
            Inventory = SerializeInventory(inv),
            EquippedLeft = inv.equippedLeft,
            EquippedRight = inv.equippedRight
        };

        Conn.InsertOrReplace(row);
        // Debug.Log($"[Save] {DbFile}");
    }

    public static bool LoadProgress(out string scene, out string spawn)
    {
        scene = null; spawn = null;
        var row = Conn.Find<SaveRow>("Autosave");    // PrimaryKey lookup
        if (row == null) return false;
        scene = row.Scene;
        spawn = row.SpawnId;
        return true;
    }

public static bool TryLoadIntoPlayer(PlayerInventory inv, PlayerStats stats, Transform player, bool applyPosition)
{
    var row = Conn.Find<SaveRow>("Autosave");
    if (row == null) return false;

    // 1) Position (optional)
    if (applyPosition && player)
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        player.position = new Vector3(row.Px, row.Py, row.Pz);
        if (cc) cc.enabled = true;
    }

    // 2) Health
    if (stats)
    {
        int delta = row.Health - stats.CurrentHealth;
        if (delta > 0) stats.Heal(delta);
        else if (delta < 0) stats.TakeDamage(-delta);
    }

    // 3) Inventory + equipped indices
    if (inv)
    {
        var db = ItemDatabase.LoadDefault();
        for (int i = 0; i < inv.slots.Length; i++) inv.slots[i] = null;

        if (!string.IsNullOrEmpty(row.Inventory))
        {
            var parts = row.Inventory.Split('|');
            for (int i = 0; i < inv.slots.Length && i < parts.Length; i++)
            {
                var key = parts[i];
                if (string.IsNullOrEmpty(key)) { inv.slots[i] = null; continue; }
                var found = db?.GetById(key) ?? db?.GetByName(key);
#if UNITY_EDITOR
                if (!found) Debug.LogWarning($"[Load] Missing item '{key}' in ItemDatabase.");
#endif
                inv.slots[i] = found;
            }
        }

        inv.equippedLeft  = row.EquippedLeft;
        inv.equippedRight = row.EquippedRight;

        Debug.Log($"[Load] {row.Scene}@{row.SpawnId} inv='{row.Inventory}' " +
                  $"left={inv.equippedLeft} right={inv.equippedRight}");
    }

    return true;
}

    public static void DeleteSave()
    {
        Conn.Delete<SaveRow>("Autosave");
    }

    // ---------- helpers ----------
    static string SerializeInventory(PlayerInventory inv)
    {
        System.Text.StringBuilder sb = new();
        for (int i = 0; i < inv.slots.Length; i++)
        {
            if (i > 0) sb.Append('|');
            var d = inv.slots[i];
            if (!d) continue;
            sb.Append(string.IsNullOrEmpty(d.itemId) ? d.itemName : d.itemId);
        }
        return sb.ToString();
    }
}
