using System.Collections;
using UnityEngine;

public class Boss_DarkKnight : Monster
{
    #region Serialized Fields

    [Header("Dark Knight Patterns")]
    [SerializeField] private float dashSpeed = 10.0f;
    [SerializeField] private float dashDistance = 3.0f;
    [SerializeField] private float slashRange = 2.5f;
    [SerializeField] private int slashDamage = 25;

    #endregion

    #region Private Fields

    private bool _isPatternRunning = false;
    private static readonly int AnimAttackTrigger = Animator.StringToHash("2_Attack");

    #endregion

    #region Lifecycle

    protected override void Update()
    {
        base.Update();

        if (ShouldStartPattern())
        {
            StartCoroutine(PatternCycle());
        }
    }

    #endregion

    #region Pattern Logic

    private bool ShouldStartPattern()
    {
        if (_isPatternRunning || PlayerUnit.Instance == null || !IsInActiveSwarm()) return false;
        
        float sqrDist = (transform.position - PlayerUnit.Instance.transform.position).sqrMagnitude;
        return sqrDist < 64.0f; 
    }

    private IEnumerator PatternCycle()
    {
        _isPatternRunning = true;

        while (IsInActiveSwarm())
        {
            
            yield return new WaitForSeconds(Random.Range(2.0f, 3.5f));

            if (PlayerUnit.Instance != null && PlayerUnit.Instance.IsTransitioning) continue;

            int rand = Random.Range(0, 2);
            if (rand == 0) yield return Pattern_DashAttack();
            else yield return Pattern_WideSlash();
        }
        
        _isPatternRunning = false;
    }

    private IEnumerator Pattern_DashAttack()
    {
        IsMoveStop = true; 
        if (animator != null) animator.SetTrigger(AnimAttackTrigger);
        
        
        Vector3 startPos = transform.position;
        Vector3 backPos = startPos + Vector3.right * 0.5f;
        
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2.5f;
            transform.position = Vector3.Lerp(startPos, backPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.3f); 

        
        Vector3 dashTarget = transform.position + Vector3.left * dashDistance;
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * dashSpeed;
            transform.position = Vector3.Lerp(backPos, dashTarget, t);
            
            if (CheckPlayerCollision()) break;
            yield return null;
        }

        IsMoveStop = false;
    }

    private IEnumerator Pattern_WideSlash()
    {
        IsMoveStop = true;
        if (animator != null) animator.SetTrigger(AnimAttackTrigger);
        
        
        yield return new WaitForSeconds(0.7f); 

        if (PlayerUnit.Instance != null)
        {
            float sqrDist = (transform.position - PlayerUnit.Instance.transform.position).sqrMagnitude;
            if (sqrDist <= slashRange * slashRange)
            {
                PlayerUnit.Instance.TakeDamage(slashDamage);
                if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.2f, 0.1f);
                if (StageManager.Instance != null) StageManager.Instance.TriggerHitStop(0.1f);
            }
        }

        yield return new WaitForSeconds(0.7f); 
        IsMoveStop = false;
    }

    private bool CheckPlayerCollision()
    {
        if (PlayerUnit.Instance == null) return false;
        
        float sqrDist = (transform.position - PlayerUnit.Instance.transform.position).sqrMagnitude;
        if (sqrDist < 0.64f)
        {
            PlayerUnit.Instance.TakeDamage(slashDamage);
            if (CameraManager.Instance != null) CameraManager.Instance.Shake(0.3f, 0.2f);
            return true;
        }
        return false;
    }

    #endregion
}

