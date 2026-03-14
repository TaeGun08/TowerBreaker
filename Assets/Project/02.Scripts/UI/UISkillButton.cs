using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISkillButton : MonoBehaviour
{
    [SerializeField] private int skillSlotIndex; // 0 또는 1
    [SerializeField] private Image iconImage; // 아이콘 복구
    [SerializeField] private Image cooldownImage;
    [SerializeField] private Button button;

    private SkillInstance _assignedInstance;

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(OnClickButton);
    }

    public void SetSkill(SkillInstance instance)
    {
        _assignedInstance = instance;
        
        // 스킬 인스턴스가 있을 때만 버튼 활성화
        gameObject.SetActive(instance != null);

        if (instance != null)
        {
            if (iconImage != null)
            {
                iconImage.sprite = instance.Data.icon;
                iconImage.gameObject.SetActive(true);
            }
            
            if (button != null)
            {
                button.interactable = true; // 강제 활성화 시도
            }
        }
    }

    private void Update()
    {
        if (_assignedInstance == null || button == null) return;

        // 쿨다운 연출
        bool isReady = _assignedInstance.IsReady;
        if (cooldownImage != null)
        {
            float remaining = Mathf.Max(0, (_assignedInstance.LastUsedTime + _assignedInstance.Data.cooldown) - Time.time);
            cooldownImage.fillAmount = remaining / _assignedInstance.Data.cooldown;
            cooldownImage.gameObject.SetActive(!isReady); // 쿨타임 중일 때만 오버레이 표시
        }

        // 입력 제한 상황이 아닐 때만 버튼 활성화
        button.interactable = isReady && (PlayerUnit.Instance != null && !PlayerUnit.Instance.IsTransitioning);
    }

    private void OnClickButton()
    {
        if (_assignedInstance != null && _assignedInstance.IsReady)
        {
            PlayerUnit.Instance.UseSkill(skillSlotIndex);
        }
    }
}
