// SaveManager.cs  (sqlite-net version)
// Requires the "SQLite" plugin (sqlite_csharp) you have in Assets/SQLite
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using SQLite; // <-- IMPORTANT: this is the sqlite-net namespace

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

    public static bool TryLoadIntoPlayer(PlayerInventory inv, PlayerStats stats, Transform player)
    {
        var row = Conn.Find<SaveRow>("Autosave");
        if (row == null) return false;

        // position (optional; you usually use SpawnPoints)
        if (player)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;
            player.position = new Vector3(row.Px, row.Py, row.Pz);
            if (cc) cc.enabled = true;
        }

        if (stats) stats.Heal(stats.MaxHealth); // or set directly if you add a setter

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
                    if (string.IsNullOrEmpty(key)) continue;
                    inv.slots[i] = db?.GetById(key) ?? db?.GetByName(key);
                }
            }
            inv.equippedLeft = row.EquippedLeft;
            inv.equippedRight = row.EquippedRight;
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
