using System.Collections;
using UnityEngine;

public class StageScroller : MonoBehaviour
{
    public bool IsMoving { get; private set; }

    public void Scroll(float distance, float duration)
    {
        if (IsMoving) return;
        StartCoroutine(MoveDownCoroutine(distance, duration));
    }
    
    private IEnumerator MoveDownCoroutine(float distance, float duration)
    {
        IsMoving = true;

        Vector3 startPosition = transform.localPosition;
        Vector3 targetPosition = startPosition + Vector3.down * distance;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        transform.localPosition = targetPosition;
        IsMoving = false;
    }
}
