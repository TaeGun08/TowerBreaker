using System.Collections;
using TMPro;
using UnityEngine;

public class InGameUIManager : SingletonBase<InGameUIManager>
{
    [Header("Player Info")]
    [SerializeField] private TextMeshProUGUI hpText;

    [Header("Stage Info")]
    [SerializeField] private TextMeshProUGUI stageText;

    [Header("Stat Info")]
    [SerializeField] private TextMeshProUGUI statsText; // 전체 스탯 리스트
    [SerializeField] private TextMeshProUGUI upgradeNoticeText; // 이번에 상승한 스탯 알림

    [Header("Damage Font Settings")]
    [SerializeField] private DamageText damageTextPrefab;
    [SerializeField] private Vector3 damageTextOffset = new Vector3(0, 1.2f, 0); 
    
    private RectTransform _rectTransform;
    private Canvas _parentCanvas;

    protected override void Awake()
    {
        dontDestroy = false;
        base.Awake();
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
        if (_parentCanvas == null) _parentCanvas = GetComponentInParent<Canvas>();
    }

    private void Start()
    {
        if (PlayerUnit.Instance != null)
        {
            UpdateHP(PlayerUnit.Instance.CurrentHP, PlayerUnit.Instance.Stats.maxHp);
            UpdateStatsDisplay();
        }

        if (StageManager.Instance != null)
        {
            UpdateStageText(StageManager.Instance.StageCount);
        }

        if (upgradeNoticeText != null) upgradeNoticeText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (PlayerUnit.Instance != null)
            PlayerUnit.Instance.OnHealthChanged += HandleHealthChanged;

        if (StageManager.Instance != null)
            StageManager.Instance.OnStageProgress += HandleStageProgress;
    }

    private void OnDisable()
    {
        if (PlayerUnit.Instance != null && !SingletonBase<PlayerUnit>.IsQuitting)
            PlayerUnit.Instance.OnHealthChanged -= HandleHealthChanged;

        if (StageManager.Instance != null && !SingletonBase<StageManager>.IsQuitting)
            StageManager.Instance.OnStageProgress -= HandleStageProgress;
    }

    private void HandleHealthChanged(int cur, int max)
    {
        UpdateHP(cur, max);
        UpdateStatsDisplay();
    }

    public void UpdateStatsDisplay()
    {
        if (statsText == null || PlayerUnit.Instance == null) return;
        var s = PlayerUnit.Instance.Stats;
        // 가독성을 위해 줄바꿈과 여백을 추가한 형식으로 변경
        statsText.text = $"<b>ATK</b>  {s.baseDamage}\n" +
                         $"<b>DEF</b>  {s.defense}\n" +
                         $"<b>CRIT</b> {Mathf.RoundToInt(s.critChance * 100)}%\n" +
                         $"<b>DBL</b>  {Mathf.RoundToInt(s.doubleHitChance * 100)}%";
    }

    public void ShowUpgradeNotice(string message)
    {
        if (upgradeNoticeText == null) return;
        
        // 텍스트 애니메이션 효과(강조)를 위해 잠시 끄고 켬
        upgradeNoticeText.gameObject.SetActive(false);
        upgradeNoticeText.text = $"<color=green>UPGRADE!</color>\n<size=120%>{message}</size>";
        upgradeNoticeText.gameObject.SetActive(true);
        
        StopCoroutine("HideNoticeAfterDelay");
        StartCoroutine("HideNoticeAfterDelay");
        
        UpdateStatsDisplay();
    }

    private IEnumerator HideNoticeAfterDelay()
    {
        yield return new WaitForSeconds(3.0f);
        if (upgradeNoticeText != null) upgradeNoticeText.gameObject.SetActive(false);
    }

    private void UpdateHP(int currentHp, int maxHp)
    {
        if (hpText != null)
        {
            hpText.text = $"HP {Mathf.Max(0, currentHp)} / {maxHp}";
        }
    }

    private void HandleStageProgress()
    {
        if (StageManager.Instance != null)
        {
            UpdateStageText(StageManager.Instance.StageCount);
        }
    }

    private void UpdateStageText(int stage)
    {
        if (stageText != null)
        {
            // stage가 0부터 시작하므로 1을 더해 표시
            stageText.text = $"FLOOR {stage + 1}";
        }
    }

    public void SpawnDamageText(Vector3 worldPos, int damage, bool isCrit)
    {
        if (damageTextPrefab == null) return;

        EnsureReferences();
        if (_rectTransform == null) return;

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 targetWorldPos = worldPos + damageTextOffset;
        Vector2 screenPos = mainCam.WorldToScreenPoint(targetWorldPos);

        Camera uiCam = null;
        if (_parentCanvas != null && _parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCam = mainCam;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, screenPos, uiCam, out Vector2 localPos))
        {
            DamageText instance = Instantiate(damageTextPrefab, transform);
            RectTransform rect = instance.transform as RectTransform;
            if (rect != null)
            {
                rect.anchoredPosition = localPos;
            }
            instance.Setup(damage, isCrit);
        }
    }
}
