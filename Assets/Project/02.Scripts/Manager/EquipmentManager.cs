using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentManager : SingletonBase<EquipmentManager>
{
    [Header("Save Settings")]
    [SerializeField] private string inventorySaveKey = "OwnedEquipment";
    [SerializeField] private string equipPrefix = "Equipped_";

    [Header("Master Database")]
    [SerializeField] private List<EquipmentData> allEquipmentDatabase; // [중요] 여기에 모든 장비 SO를 넣으세요!

    public List<EquipmentData> MasterDB => allEquipmentDatabase;

    // 런타임 데이터
    private HashSet<string> _ownedEquipmentIds = new HashSet<string>();
    private Dictionary<EquipmentType, string> _equippedEquipment = new Dictionary<EquipmentType, string>();

    public event Action OnInventoryChanged;
    public event Action OnEquipmentChanged;

    protected override void Awake()
    {
        base.Awake();
        LoadData();
    }

    private void LoadData()
    {
        // 1. 보유 목록 로드
        string savedInventory = PlayerPrefs.GetString(inventorySaveKey, "");
        if (!string.IsNullOrEmpty(savedInventory))
        {
            _ownedEquipmentIds = new HashSet<string>(savedInventory.Split(','));
        }

        // 2. 장착 목록 로드
        foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
        {
            string equippedId = PlayerPrefs.GetString(equipPrefix + type.ToString(), "");
            if (!string.IsNullOrEmpty(equippedId))
            {
                _equippedEquipment[type] = equippedId;
            }
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetString(inventorySaveKey, string.Join(",", _ownedEquipmentIds));
        foreach (EquipmentType type in Enum.GetValues(typeof(EquipmentType)))
        {
            if (_equippedEquipment.TryGetValue(type, out string id))
                PlayerPrefs.SetString(equipPrefix + type.ToString(), id);
            else
                PlayerPrefs.DeleteKey(equipPrefix + type.ToString());
        }
        PlayerPrefs.Save();
    }

    public void AddEquipment(string id)
    {
        if (_ownedEquipmentIds.Add(id))
        {
            SaveData();
            OnInventoryChanged?.Invoke();
        }
    }

    public void Equip(EquipmentData data)
    {
        if (data == null || !_ownedEquipmentIds.Contains(data.id)) return;

        _equippedEquipment[data.type] = data.id;
        SaveData();
        OnEquipmentChanged?.Invoke();
    }

    public void Unequip(EquipmentType type)
    {
        if (_equippedEquipment.Remove(type))
        {
            SaveData();
            OnEquipmentChanged?.Invoke();
        }
    }

    public EquipmentData GetEquippedItem(EquipmentType type)
    {
        if (_equippedEquipment.TryGetValue(type, out string id))
        {
            return GetEquipmentById(id);
        }
        return null;
    }

    public List<EquipmentData> GetOwnedEquipment()
    {
        return _ownedEquipmentIds.Select(GetEquipmentById).Where(e => e != null).ToList();
    }

    public EquipmentData GetEquipmentById(string id)
    {
        // 데이터베이스에서 ID로 검색
        return allEquipmentDatabase?.FirstOrDefault(e => e.id == id);
    }

    // 장착된 모든 장비의 보너스 스탯 합산
    public (int atk, int def, int hp, float crit, float doubleHit) GetTotalBonuses()
    {
        int atk = 0, def = 0, hp = 0;
        float crit = 0, dbl = 0;

        foreach (var id in _equippedEquipment.Values)
        {
            var data = GetEquipmentById(id);
            if (data != null)
            {
                atk += data.atkBonus;
                def += data.defBonus;
                hp += data.hpBonus;
                crit += data.critBonus;
                dbl += data.doubleHitBonus;
            }
        }
        return (atk, def, hp, crit, dbl);
    }
}
