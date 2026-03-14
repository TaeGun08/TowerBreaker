using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class StageManager : SingletonBase<StageManager>
{
    #region Events

    public event Action OnStageProgress;
    public event Action OnGameClear; 

    #endregion

    #region Serialized Fields

    [Header("Game State")]
    [field: SerializeField] public int StageCount { get; private set; } = 0; 
    [SerializeField] private int maxStageCount = 100;
    [SerializeField] private float autoProceedDelay = 0.8f; 
    
    #endregion

    #region Private Fields

    public bool IsTransitioning { get; private set; }
    private Swarm _currentSwarm;
    private bool _isGameOver = false; 

    #endregion

    #region Properties

    public Swarm CurrentSwarm 
    { 
        get => _currentSwarm;
        set 
        {
            if (_currentSwarm != null)
            {
                _currentSwarm.SetMoveStop(true);
                _currentSwarm.OnCleared -= HandleSwarmCleared;
            }

            _currentSwarm = value;
            if (_currentSwarm == null) return;
            
            _currentSwarm.SetMoveStop(false);
            _currentSwarm.OnCleared += HandleSwarmCleared;
        }
    }

    #endregion

    #region Lifecycle

    protected override void Awake()
    {
        dontDestroy = false;
        base.Awake();
    }

    private void Update()
    {
        
        if (!_isGameOver && PlayerUnit.Instance != null && PlayerUnit.Instance.CurrentHP <= 0)
        {
            HandleGameOver();
        }
    }

    #endregion

    #region Stage Progression

    private void HandleGameOver()
    {
        _isGameOver = true;
        Debug.Log("<color=red>Game Over! Showing Results...</color>");
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        
        yield return new WaitForSecondsRealtime(2.0f);
        
        
        if (InGameUIManager.Instance != null)
        {
            InGameUIManager.Instance.ShowGameOverPanel();
        }
    }

    private void HandleSwarmCleared()
    {
        if (PlayerUnit.Instance != null) PlayerUnit.Instance.Heal(5);

        if (StageCount >= maxStageCount - 1)
        {
            OnGameClear?.Invoke();
            
            return;
        }
        
        if (!IsTransitioning && !_isGameOver)
        {
            StartCoroutine(AutoProceedToNextFloor());
        }
    }

    private IEnumerator AutoProceedToNextFloor()
    {
        IsTransitioning = true;

        
        CollectAllRewards();
        yield return new WaitForSeconds(autoProceedDelay);

        
        yield return StartCoroutine(MovePlayerToExit());

        
        int clearedFloor = StageCount + 1;
        bool wasBoss = (clearedFloor % 5 == 0); 
        
        ProcessFloorClearRewards(wasBoss);
        
        
        NextStage(wasBoss);
        yield return new WaitForSeconds(0.5f); 
        IsTransitioning = false;
    }

    private IEnumerator MovePlayerToExit()
    {
        if (PlayerUnit.Instance == null) yield break;

        bool playerMoved = false;
        float waitTimeout = 2.0f;
        float waitElapsed = 0f;

        PlayerUnit.Instance.MoveToNextFloorSequence(() => { playerMoved = true; });
        
        while (!playerMoved && waitElapsed < waitTimeout)
        {
            waitElapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void ProcessFloorClearRewards(bool wasBoss)
    {
        if (wasBoss)
        {
            if (SkillGachaManager.Instance != null)
                SkillGachaManager.Instance.OpenGachaUI();
        }
        else if (PlayerUnit.Instance != null)
        {
            string msg = PlayerUnit.Instance.UpgradeRandomStat();
            if (InGameUIManager.Instance != null) 
                InGameUIManager.Instance.ShowUpgradeNotice(msg);
        }
    }

    public void NextStage(bool wasBoss = false)
    {
        if (StageCount < maxStageCount - 1)
        {
            StageCount++;
            OnStageProgress?.Invoke();
            Debug.Log($"<color=white>Entered Floor {StageCount + 1}</color>");
        }
    }

    #endregion

    #region Reward Handling

    private void CollectAllRewards()
    {
        
        Corpse[] corpses = FindObjectsByType<Corpse>(FindObjectsSortMode.None);
        foreach (var corpse in corpses)
        {
            if (corpse != null) corpse.StartAbsorb();
        }

        
        RewardChest[] chests = FindObjectsByType<RewardChest>(FindObjectsSortMode.None);
        foreach (var chest in chests)
        {
            if (chest != null) chest.StartAbsorb();
        }
    }

    #endregion

    #region Visual Effects & HitStop

    public void TriggerHitStop(float duration)
    {
        if (!gameObject.activeInHierarchy) return;
        StopCoroutine("HitStopCoroutine");
        StartCoroutine(HitStopCoroutine(duration));
    }

    private IEnumerator HitStopCoroutine(float duration)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f; 
    }

    #endregion
}

