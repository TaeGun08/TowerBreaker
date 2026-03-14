using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualFeedback : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer[] _spriteRenderers;

    public bool IsStunned { get; private set; }
    private static readonly int AnimDamagedTrigger = Animator.StringToHash("3_Damaged");

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void PlayHitEffect(bool canPlayAnimation = true, float stunDuration = 0.1f)
    {
        StartCoroutine(HitSequence(canPlayAnimation, stunDuration));
    }

    private IEnumerator HitSequence(bool canPlayAnimation, float stunDuration)
    {
        IsStunned = true;
        
        if (canPlayAnimation && _animator != null)
        {
            _animator.SetTrigger(AnimDamagedTrigger);
        }

        if (_spriteRenderers != null && _spriteRenderers.Length > 0)
        {
            List<Color> originalColors = new List<Color>();
            foreach (var sr in _spriteRenderers)
            {
                if (sr == null) continue;
                originalColors.Add(sr.color);
                
                
                sr.color = new Color(1.5f, 1.5f, 1.5f, sr.color.a); 
            }

            yield return new WaitForSeconds(0.05f);

            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                if (_spriteRenderers[i] != null)
                {
                    _spriteRenderers[i].color = originalColors[i];
                }
            }
        }

        if (stunDuration > 0.05f)
        {
            yield return new WaitForSeconds(stunDuration - 0.05f);
        }

        IsStunned = false;
    }
}

