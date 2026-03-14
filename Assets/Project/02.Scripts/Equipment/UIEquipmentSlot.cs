using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIEquipmentSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private GameObject equippedCheck;
    [SerializeField] private Button button;

    private EquipmentData _data;
    private System.Action<EquipmentData> _onSelected;
    private System.Action<EquipmentData> _onDoubleClicked;

    public void Setup(EquipmentData data, bool isEquipped, System.Action<EquipmentData> onSelected, System.Action<EquipmentData> onDoubleClicked = null)
    {
        _data = data;
        _onSelected = onSelected;
        _onDoubleClicked = onDoubleClicked;

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.gameObject.SetActive(data.icon != null);
        }
        
        if (frameImage != null) frameImage.color = data.GetTierColor();
        if (equippedCheck != null) equippedCheck.SetActive(isEquipped);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => _onSelected?.Invoke(_data));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
        {
            _onDoubleClicked?.Invoke(_data);
        }
    }
}
