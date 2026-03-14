using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEquipmentPanel : MonoBehaviour
{
    [Header("Required Parents")]
    [SerializeField] private Transform contentParent; // 인벤토리 리스트 부모
    [SerializeField] private UIEquipmentSlot slotPrefab;

    // 내부에서 자동으로 찾을 요소들
    private TextMeshProUGUI itemNameText;
    private TextMeshProUGUI itemStatsText;
    private TextMeshProUGUI equipButtonText;
    private Button equipButton;
    private Button backButton;

    private Dictionary<EquipmentType, UIEquipmentSlot> _equippedSlots = new Dictionary<EquipmentType, UIEquipmentSlot>();
    private EquipmentData _selectedItem;

    private void Awake()
    {
        // 프리팹 및 부모 객체 자동 할당
        if (slotPrefab == null) slotPrefab = Resources.Load<UIEquipmentSlot>("Prefabs/UI/EquipmentSlot");
        if (contentParent == null) contentParent = transform.FindDeepChild("ContentParent");

        AutoAssignUI();
        
        if (equipButton != null) equipButton.onClick.AddListener(OnClickEquip);
        if (backButton != null) backButton.onClick.AddListener(OnClickBack);
    }

    private void AutoAssignUI()
    {
        // 이름 기반으로 자식 오브젝트 자동 찾기
        itemNameText = FindChild<TextMeshProUGUI>("Text_ItemName");
        itemStatsText = FindChild<TextMeshProUGUI>("Text_ItemStats");
        equipButton = FindChild<Button>("Btn_Equip");
        if (equipButton != null) equipButtonText = equipButton.GetComponentInChildren<TextMeshProUGUI>();
        backButton = FindChild<Button>("Btn_Back");

        // 부위별 장착 슬롯 자동 매핑 (이름 규칙: Slot_Weapon, Slot_Armor 등)
        foreach (EquipmentType type in System.Enum.GetValues(typeof(EquipmentType)))
        {
            var slot = FindChild<UIEquipmentSlot>("Slot_" + type.ToString());
            if (slot != null) _equippedSlots[type] = slot;
        }
    }

    private T FindChild<T>(string name) where T : Component
    {
        Transform target = transform.FindDeepChild(name);
        return target != null ? target.GetComponent<T>() : null;
    }

    private void OnEnable()
    {
        RefreshUI();
        ShowDetails(null);
    }

    public void RefreshUI()
    {
        RefreshInventory();
        RefreshEquippedSlots();
    }

    public void RefreshInventory()
    {
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        var ownedItems = EquipmentManager.Instance.GetOwnedEquipment();
        foreach (var item in ownedItems)
        {
            var slot = Instantiate(slotPrefab, contentParent);
            bool isEquipped = IsItemEquipped(item);
            slot.Setup(item, isEquipped, OnItemSelected, OnItemDoubleClicked);
        }
    }

    private void RefreshEquippedSlots()
    {
        foreach (var pair in _equippedSlots)
        {
            SetupEquippedSlot(pair.Value, pair.Key);
        }
    }

    private void SetupEquippedSlot(UIEquipmentSlot slot, EquipmentType type)
    {
        if (slot == null) return;
        
        EquipmentData data = EquipmentManager.Instance.GetEquippedItem(type);
        if (data != null)
        {
            slot.gameObject.SetActive(true);
            slot.Setup(data, true, OnItemSelected, OnItemDoubleClicked);
        }
        else
        {
            slot.gameObject.SetActive(false);
        }
    }

    private void OnItemSelected(EquipmentData data)
    {
        _selectedItem = data;
        ShowDetails(data);
    }

    private void OnItemDoubleClicked(EquipmentData data)
    {
        // 더블 클릭 시 즉시 장착/해제 수행
        ToggleEquip(data);
    }

    private void ShowDetails(EquipmentData data)
    {
        if (data == null)
        {
            itemNameText.text = "Select Item";
            itemStatsText.text = "";
            equipButton.gameObject.SetActive(false);
            return;
        }

        equipButton.gameObject.SetActive(true);
        itemNameText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(data.GetTierColor())}>{data.equipmentName}</color>";
        
        string stats = "";
        if (data.atkBonus != 0) stats += $"ATK +{data.atkBonus}\n";
        if (data.defBonus != 0) stats += $"DEF +{data.defBonus}\n";
        if (data.hpBonus != 0) stats += $"HP +{data.hpBonus}\n";
        if (data.critBonus != 0) stats += $"CRIT +{data.critBonus * 100}%\n";
        if (data.doubleHitBonus != 0) stats += $"DOUBLE +{data.doubleHitBonus * 100}%\n";
        itemStatsText.text = stats;

        bool isEquipped = IsItemEquipped(data);
        equipButtonText.text = isEquipped ? "Unequip" : "Equip";
    }

    public void OnClickEquip()
    {
        if (_selectedItem == null) return;
        ToggleEquip(_selectedItem);
    }

    private void ToggleEquip(EquipmentData data)
    {
        if (data == null) return;

        if (IsItemEquipped(data))
        {
            EquipmentManager.Instance.Unequip(data.type);
        }
        else
        {
            EquipmentManager.Instance.Equip(data);
        }

        RefreshUI();
        ShowDetails(data);
    }

    private bool IsItemEquipped(EquipmentData data)
    {
        var equipped = EquipmentManager.Instance.GetEquippedItem(data.type);
        return equipped != null && equipped.id == data.id;
    }

    public void OnClickBack()
    {
        OutGameManager.Instance.ShowMainPanel();
    }
}
