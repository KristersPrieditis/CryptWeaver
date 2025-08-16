#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class ItemIconBaker : EditorWindow
{
    const int DefaultSize = 512;
    const string OutputFolder = "Assets/Icons";

    int size = DefaultSize;
    Color bgColor = new Color(0, 0, 0, 0);
    bool orthographic = true;
    Vector3 cameraEuler = new Vector3(20f, -30f, 0f);
    float padding = 1.15f;
    float lightIntensity = 1.2f;

    [MenuItem("MORTIS/Icon Baker")]
    public static void ShowWindow() => GetWindow<ItemIconBaker>("Icon Baker");

    void OnGUI()
    {
        GUILayout.Label("Render Settings", EditorStyles.boldLabel);
        size = EditorGUILayout.IntSlider("Size (px)", size, 64, 2048);
        bgColor = EditorGUILayout.ColorField("Background", bgColor);
        orthographic = EditorGUILayout.Toggle("Orthographic", orthographic);
        cameraEuler = EditorGUILayout.Vector3Field("Camera Rotation", cameraEuler);
        padding = EditorGUILayout.Slider("Padding/Zoom", padding, 1.0f, 2.0f);
        lightIntensity = EditorGUILayout.Slider("Light Intensity", lightIntensity, 0f, 3f);

        EditorGUILayout.Space();
        if (GUILayout.Button("Bake Icons for All ItemData"))
            BakeAll();
    }

    void BakeAll()
    {
        Directory.CreateDirectory(OutputFolder);
        var guids = AssetDatabase.FindAssets("t:ItemData");
        int count = 0;

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var itemData = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (!itemData) continue;

            var prefab = GetPrefabFromItemData(itemData);
            if (!prefab)
            {
                Debug.LogWarning($"[{path}] has no prefab assigned; skipping.");
                continue;
            }

            string baseName = Path.GetFileNameWithoutExtension(path);
            string iconPath = $"{OutputFolder}/{Sanitize(baseName)}.png";

            var tex = RenderPrefabToTexture(prefab);
            if (!tex) continue;

            File.WriteAllBytes(iconPath, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(iconPath, ImportAssetOptions.ForceUpdate);

            // Import as Sprite
            var ti = (TextureImporter)AssetImporter.GetAtPath(iconPath);
            ti.textureType = TextureImporterType.Sprite;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.spritePixelsPerUnit = 100;
            ti.filterMode = FilterMode.Bilinear;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);

            // If ItemData has a 'public Sprite icon;' field, assign it
            var iconField = itemData.GetType().GetField("icon");
            if (iconField != null && iconField.FieldType == typeof(Sprite))
            {
                iconField.SetValue(itemData, sprite);
                EditorUtility.SetDirty(itemData);
            }

            count++;
            Object.DestroyImmediate(tex);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Icon Baker", $"Baked {count} icons.", "OK");
    }

    static string Sanitize(string s)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
        return s.Replace(' ', '_');
    }

    static GameObject GetPrefabFromItemData(ScriptableObject itemData)
    {
        var field = itemData.GetType().GetField("prefab");
        if (field != null && typeof(GameObject).IsAssignableFrom(field.FieldType))
            return (GameObject)field.GetValue(itemData);
        return null;
    }

    Texture2D RenderPrefabToTexture(GameObject prefab)
    {
        PreviewRenderUtility pru = null;
        GameObject instance = null, lightGO = null;

        try
        {
            pru = new PreviewRenderUtility();
            var cam = pru.camera; // Unity 6 still exposes this
            cam.nearClipPlane = 0.01f;
            cam.farClipPlane  = 100f;
            cam.clearFlags    = CameraClearFlags.SolidColor;
            cam.backgroundColor = bgColor;

            instance = (GameObject)Object.Instantiate(prefab);
            instance.hideFlags = HideFlags.HideAndDontSave;
            instance.transform.rotation = Quaternion.identity;

            var rends = instance.GetComponentsInChildren<Renderer>();
            if (rends.Length == 0) return null;

            Bounds b = new Bounds(rends[0].bounds.center, Vector3.zero);
            foreach (var r in rends) b.Encapsulate(r.bounds);

            cam.transform.rotation = Quaternion.Euler(cameraEuler);
            float halfSize = Mathf.Max(b.extents.x, b.extents.y, b.extents.z) * padding;

            if (orthographic)
            {
                cam.orthographic = true;
                cam.orthographicSize = Mathf.Max(halfSize, 0.001f);
                cam.transform.position = b.center - cam.transform.forward * 5f;
            }
            else
            {
                cam.orthographic = false;
                float fov = 30f;
                cam.fieldOfView = fov;
                float dist = halfSize / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
                cam.transform.position = b.center - cam.transform.forward * Mathf.Max(dist, 0.3f);
            }
            cam.transform.LookAt(b.center);

            lightGO = new GameObject("KeyLight") { hideFlags = HideFlags.HideAndDontSave };
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = lightIntensity;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            pru.AddSingleGO(instance);
            pru.AddSingleGO(lightGO);

            var rect = new Rect(0, 0, size, size);
            // Static preview returns a Texture2D we can save as PNG
            pru.BeginStaticPreview(rect);
            pru.camera.Render();
            Texture2D tex = pru.EndStaticPreview();  // <- Texture2D
            if (tex != null) tex.name = prefab.name + "_Icon";
            return tex;

        }
        finally
        {
            if (pru != null) pru.Cleanup();
            if (instance) Object.DestroyImmediate(instance);
            if (lightGO) Object.DestroyImmediate(lightGO);
        }
    }
}
#endif
