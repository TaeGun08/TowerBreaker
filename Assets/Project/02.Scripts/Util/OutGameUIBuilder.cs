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
        // 1. 기존 UI 제거 (중복 생성 방지)
        var oldCanvas = GameObject.Find("OutGame_Canvas");
        if (oldCanvas) DestroyImmediate(oldCanvas);
        var oldMgr = GameObject.Find("Managers");
        if (oldMgr) DestroyImmediate(oldMgr);

        // 2. Managers 생성 및 데이터베이스 자동 로드
        GameObject managersObj = new GameObject("Managers");
        OutGameManager outMgr = managersObj.AddComponent<OutGameManager>();
        EquipmentManager eqMgr = managersObj.AddComponent<EquipmentManager>();
        EquipmentGachaManager gachaMgr = managersObj.AddComponent<EquipmentGachaManager>();

        // 모든 장비 SO 로드 및 등록
        string[] guids = AssetDatabase.FindAssets("t:EquipmentData");
        List<EquipmentData> allEquips = new List<EquipmentData>();
        foreach (string guid in guids)
        {
            allEquips.Add(AssetDatabase.LoadAssetAtPath<EquipmentData>(AssetDatabase.GUIDToAssetPath(guid)));
        }
        
        SerializedObject eqSo = new SerializedObject(eqMgr);
        SerializedProperty dbProp = eqSo.FindProperty("allEquipmentDatabase");
        dbProp.ClearArray();
        for (int i = 0; i < allEquips.Count; i++)
        {
            dbProp.InsertArrayElementAtIndex(i);
            dbProp.GetArrayElementAtIndex(i).objectReferenceValue = allEquips[i];
        }
        eqSo.ApplyModifiedProperties();

        // 3. Canvas 생성
        GameObject canvasObj = new GameObject("OutGame_Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(TARGET_WIDTH, TARGET_HEIGHT);
        scaler.matchWidthOrHeight = 1.0f;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject panelsObj = CreateUIObject("Panels", canvasObj.transform);
        SetRectFullStretch(panelsObj.GetComponent<RectTransform>());

        // 4. 슬롯 프리팹 확인 및 자동 생성 (없을 경우 대비)
        UIEquipmentSlot slotPrefab = EnsureSlotPrefab();

        // --- 5. Main Panel ---
        GameObject mainPanel = CreatePanel("MainPanel", panelsObj.transform);
        GameObject mainBtnGroup = CreateUIObject("MainButtonGroup", mainPanel.transform);
        SetRectSize(mainBtnGroup.GetComponent<RectTransform>(), 1000, 1200, new Vector2(0.5f, 0.4f));
        
        var btnStart = CreateButton("Btn_Start", "GAME START", mainBtnGroup.transform, 200);
        var btnEquip = CreateButton("Btn_Equipment", "EQUIPMENT", mainBtnGroup.transform, 200);
        var btnGacha = CreateButton("Btn_Gacha", "GACHA SHOP", mainBtnGroup.transform, 200);
        var btnQuit = CreateButton("Btn_Quit", "QUIT", mainBtnGroup.transform, 200);
        
        mainBtnGroup.AddComponent<VerticalLayoutGroup>().spacing = 50;

        // 이벤트 연결 (UnityEventTools 사용)
        UnityEventTools.AddPersistentListener(btnStart.onClick, outMgr.StartGame);
        UnityEventTools.AddPersistentListener(btnEquip.onClick, outMgr.ShowEquipmentPanel);
        UnityEventTools.AddPersistentListener(btnGacha.onClick, outMgr.ShowGachaPanel);
        UnityEventTools.AddPersistentListener(btnQuit.onClick, outMgr.QuitGame);

        // --- 6. Equipment Panel ---
        GameObject equipPanel = CreatePanel("EquipmentPanel", panelsObj.transform);
        UIEquipmentPanel uiEquip = equipPanel.AddComponent<UIEquipmentPanel>();
        
        GameObject invArea = CreateUIObject("InventoryArea", equipPanel.transform);
        SetRectSize(invArea.GetComponent<RectTransform>(), 1200, 1000, new Vector2(0.5f, 0.65f));
        GameObject contentParent = CreateUIObject("ContentParent", invArea.transform);
        contentParent.AddComponent<GridLayoutGroup>().cellSize = new Vector2(250, 250);
        
        GameObject detailArea = CreateUIObject("DetailArea", equipPanel.transform);
        SetRectSize(detailArea.GetComponent<RectTransform>(), 1200, 400, new Vector2(0.5f, 0.35f));
        CreateText("Text_ItemName", "Item Name", detailArea.transform, 80, new Vector2(0.5f, 0.8f));
        CreateText("Text_ItemStats", "Stats", detailArea.transform, 60, new Vector2(0.5f, 0.4f));
        var btnDoEquip = CreateButton("Btn_Equip", "EQUIP", detailArea.transform, 150, new Vector2(0.5f, 0.1f));
        
        var btnBackEquip = CreateButton("Btn_Back", "BACK", equipPanel.transform, 120, new Vector2(0.15f, 0.95f));
        UnityEventTools.AddPersistentListener(btnBackEquip.onClick, outMgr.ShowMainPanel);
        UnityEventTools.AddPersistentListener(btnDoEquip.onClick, uiEquip.OnClickEquip);

        // --- 7. Gacha Panel ---
        GameObject gachaPanel = CreatePanel("GachaPanel", panelsObj.transform);
        UIGachaPanel uiGacha = gachaPanel.AddComponent<UIGachaPanel>();
        
        CreateText("Text_TotalChest", "CHESTS: 0", gachaPanel.transform, 100, new Vector2(0.5f, 0.85f));
        var btnDoGacha = CreateButton("Btn_Draw", "OPEN CHEST", gachaPanel.transform, 250, new Vector2(0.5f, 0.6f));
        
        GameObject resArea = CreateUIObject("ResultArea", gachaPanel.transform);
        SetRectSize(resArea.GetComponent<RectTransform>(), 1000, 800, new Vector2(0.5f, 0.35f));
        CreateUIObject("Img_Frame", resArea.transform).AddComponent<Image>();
        CreateUIObject("Img_ItemIcon", resArea.transform).AddComponent<Image>();
        CreateText("Text_Tier", "[TIER]", resArea.transform, 70, new Vector2(0.5f, 0.2f));
        CreateText("Text_ItemName", "Item Name", resArea.transform, 90, new Vector2(0.5f, 0.1f));
        CreateText("Text_ItemStats", "Stats", resArea.transform, 60, new Vector2(0.5f, 0.0f));
        
        var btnBackGacha = CreateButton("Btn_Back", "BACK", gachaPanel.transform, 120, new Vector2(0.15f, 0.95f));
        UnityEventTools.AddPersistentListener(btnBackGacha.onClick, outMgr.ShowMainPanel);
        UnityEventTools.AddPersistentListener(btnDoGacha.onClick, uiGacha.OnClickDraw);

        // 8. 매니저 참조 최종 연결
        SerializedObject outSo = new SerializedObject(outMgr);
        outSo.FindProperty("mainPanel").objectReferenceValue = mainPanel;
        outSo.FindProperty("equipmentPanel").objectReferenceValue = equipPanel;
        outSo.FindProperty("gachaPanel").objectReferenceValue = gachaPanel;
        outSo.ApplyModifiedProperties();

        SerializedObject uiEquipSo = new SerializedObject(uiEquip);
        uiEquipSo.FindProperty("contentParent").objectReferenceValue = contentParent.transform;
        uiEquipSo.FindProperty("slotPrefab").objectReferenceValue = slotPrefab;
        uiEquipSo.ApplyModifiedProperties();

        Debug.Log("<color=orange>DONE! All UI, Managers, and Events are fully linked. PRESS PLAY!</color>");
        Selection.activeGameObject = canvasObj;
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
