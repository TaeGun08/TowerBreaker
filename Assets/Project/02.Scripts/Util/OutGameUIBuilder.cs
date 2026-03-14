#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using System.Collections.Generic;

public class OutGameUIBuilder : EditorWindow
{
    private const float TARGET_WIDTH = 1440f;
    private const float TARGET_HEIGHT = 2560f;

    [MenuItem("TowerBreaker/Build OutGame UI (ALL-IN-ONE)")]
    public static void BuildUI()
    {
        
    }

    [MenuItem("TowerBreaker/Clear All Save Data")]
    public static void ClearSaveData()
    {
        if (EditorUtility.DisplayDialog("Clear Save Data", "Are you sure you want to delete ALL save data (Chests, Equipment, etc)?", "Yes", "No"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }

    private static UIEquipmentSlot EnsureSlotPrefab()
    {
        string path = "Assets/Project/Resources/Prefabs/UI/EquipmentSlot.prefab";
        UIEquipmentSlot prefab = AssetDatabase.LoadAssetAtPath<UIEquipmentSlot>(path);
        
        if (prefab == null)
        {
            GameObject obj = new GameObject("EquipmentSlot");
            obj.AddComponent<RectTransform>().sizeDelta = new Vector2(250, 250);
            obj.AddComponent<Image>().color = Color.gray;
            var slot = obj.AddComponent<UIEquipmentSlot>();
            
            GameObject icon = new GameObject("Icon");
            icon.transform.SetParent(obj.transform, false);
            icon.AddComponent<Image>();
            
            GameObject frame = new GameObject("Frame");
            frame.transform.SetParent(obj.transform, false);
            frame.AddComponent<Image>();
            
            obj.AddComponent<Button>();
            
            if (!System.IO.Directory.Exists("Assets/Project/Resources/Prefabs/UI"))
                System.IO.Directory.CreateDirectory("Assets/Project/Resources/Prefabs/UI");
                
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(obj, path);
            DestroyImmediate(obj);
            prefab = savedPrefab.GetComponent<UIEquipmentSlot>();
        }
        return prefab;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.AddComponent<RectTransform>();
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static void SetRectFullStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    private static void SetRectSize(RectTransform rt, float w, float h, Vector2 anchorPos)
    {
        rt.anchorMin = anchorPos; rt.anchorMax = anchorPos;
        rt.sizeDelta = new Vector2(w, h); rt.anchoredPosition = Vector2.zero;
    }

    private static GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = CreateUIObject(name, parent);
        panel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.15f, 1.0f);
        SetRectFullStretch(panel.GetComponent<RectTransform>());
        return panel;
    }

    private static Button CreateButton(string name, string label, Transform parent, float height, Vector2? anchor = null)
    {
        GameObject btnObj = CreateUIObject(name, parent);
        btnObj.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.4f, 1.0f);
        Button btn = btnObj.AddComponent<Button>();
        
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        if (anchor.HasValue) { rt.anchorMin = anchor.Value; rt.anchorMax = anchor.Value; }
        rt.sizeDelta = new Vector2(800, height);
        rt.anchoredPosition = Vector2.zero;
        
        CreateText("Text", label, btnObj.transform, height * 0.4f);
        return btn;
    }

    private static void CreateText(string name, string content, Transform parent, float fontSize, Vector2? anchor = null)
    {
        GameObject txtObj = CreateUIObject(name, parent);
        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        
        if (anchor.HasValue)
        {
            RectTransform rt = txtObj.GetComponent<RectTransform>();
            rt.anchorMin = anchor.Value; rt.anchorMax = anchor.Value;
            rt.sizeDelta = new Vector2(1200, fontSize * 1.5f);
            rt.anchoredPosition = Vector2.zero;
        }
    }
}
#endif
