using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEquipmentPanel : MonoBehaviour
{
    [Header("Manual Assignments")]
    [SerializeField] private Transform contentParent; // 인벤토리 리스트 부모
    [SerializeField] private UIEquipmentSlot slotPrefab;

    // 내부 자동 할당 요소들
    private UIEquipmentSlot weaponSlot;
    private UIEquipmentSlot armorSlot;
    private UIEquipmentSlot helmetSlot;
    private UIEquipmentSlot shoesSlot;
    private UIEquipmentSlot accessorySlot;

    private TextMeshProUGUI itemNameText;
    private TextMeshProUGUI itemStatsText;
    private Button equipButton;
    private TextMeshProUGUI equipButtonText;
    private Button backButton;

    private EquipmentData _selectedItem;
    private Dictionary<EquipmentType, UIEquipmentSlot> _equippedSlots = new Dictionary<EquipmentType, UIEquipmentSlot>();

    private void Awake()
    {
        AutoAssignUI();
        
        if (equipButton != null) equipButton.onClick.AddListener(OnClickEquip);
        if (backButton != null) backButton.onClick.AddListener(OnClickBack);
    }

    private void AutoAssignUI()
    {
        // 텍스트 및 버튼 자동 찾기
        itemNameText = FindChild<TextMeshProUGUI>("ItemName");
        itemStatsText = FindChild<TextMeshProUGUI>("ItemStats");
        equipButton = FindChild<Button>("Equip_Button");
        if (equipButton != null) equipButtonText = equipButton.GetComponentInChildren<TextMeshProUGUI>();
        backButton = FindChild<Button>("Back_Button");

        // 부위별 슬롯 자동 찾기 (이름 규칙: Slot_Weapon, Slot_Armor 등)
        _equippedSlots.Clear();
        weaponSlot = FindChild<UIEquipmentSlot>("Slot_Weapon");
        armorSlot = FindChild<UIEquipmentSlot>("Slot_Armor");
        helmetSlot = FindChild<UIEquipmentSlot>("Slot_Helmet");
        shoesSlot = FindChild<UIEquipmentSlot>("Slot_Shoes");
        accessorySlot = FindChild<UIEquipmentSlot>("Slot_Accessory");

        if (weaponSlot != null) _equippedSlots[EquipmentType.Weapon] = weaponSlot;
        if (armorSlot != null) _equippedSlots[EquipmentType.Armor] = armorSlot;
        if (helmetSlot != null) _equippedSlots[EquipmentType.Helmet] = helmetSlot;
        if (shoesSlot != null) _equippedSlots[EquipmentType.Shoes] = shoesSlot;
        if (accessorySlot != null) _equippedSlots[EquipmentType.Accessory] = accessorySlot;
    }

    private T FindChild<T>(string name) where T : Component
    {
        Transform target = transform.FindDeepChild(name);
        return target != null ? target.GetComponent<T>() : null;
    }

    private void OnEnable()
    {
        // 참조 재검증
        if (itemNameText == null) AutoAssignUI();
        
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
        if (contentParent == null) return;

        foreach (Transform child in contentParent) Destroy(child.gameObject);

        var ownedItems = EquipmentManager.Instance.GetOwnedEquipment();
        foreach (var item in ownedItems)
        {
            if (item == null) continue;
            var slot = Instantiate(slotPrefab, contentParent);
            bool isEquipped = IsItemEquipped(item);
            slot.Setup(item, isEquipped, 
                (data) => OnItemSelected(data), 
                (data) => OnItemDoubleClicked(data));
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
            slot.Setup(data, true, (d) => OnItemSelected(d), (d) => OnItemDoubleClicked(d));
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
        _selectedItem = data;
        ToggleEquip(data);
    }

    private void ShowDetails(EquipmentData data)
    {
        if (data == null)
        {
            if (itemNameText != null) itemNameText.text = "Select Item";
            if (itemStatsText != null) itemStatsText.text = "";
            if (equipButton != null) equipButton.gameObject.SetActive(false);
            return;
        }

        if (equipButton != null) equipButton.gameObject.SetActive(true);
        if (itemNameText != null) 
            itemNameText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(data.GetTierColor())}>{data.equipmentName}</color>";
        
        string stats = "";
        if (data.atkBonus != 0) stats += $"ATK +{data.atkBonus}\n";
        if (data.defBonus != 0) stats += $"DEF +{data.defBonus}\n";
        if (data.hpBonus != 0) stats += $"HP +{data.hpBonus}\n";
        if (data.critBonus != 0) stats += $"CRIT +{data.critBonus * 100}%\n";
        if (data.doubleHitBonus != 0) stats += $"DOUBLE +{data.doubleHitBonus * 100}%\n";
        
        if (itemStatsText != null) itemStatsText.text = stats;

        bool isEquipped = IsItemEquipped(data);
        if (equipButtonText != null) equipButtonText.text = isEquipped ? "Unequip" : "Equip";
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
