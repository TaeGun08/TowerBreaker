using System.Collections;
using UnityEngine;

public class UIAbsorber : SingletonBase<UIAbsorber>
{
    [Header("Target UI Transforms")]
    [SerializeField] private RectTransform goldTargetUI; 
    [SerializeField] private RectTransform chestTargetUI; 

    public void Absorb(GameObject worldObj, bool isChest, System.Action onComplete = null)
    {
        RectTransform target = isChest ? chestTargetUI : goldTargetUI;
        if (target == null)
        {
            Destroy(worldObj);
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(AbsorbRoutine(worldObj, target, onComplete));
    }

    private IEnumerator AbsorbRoutine(GameObject obj, RectTransform targetUI, System.Action onComplete)
    {
        Camera cam = Camera.main;
        Vector3 startPos = obj.transform.position;
        float elapsed = 0f;
        float duration = 0.8f;

        
        
        
        while (elapsed < duration)
        {
            if (obj == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            
            Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, targetUI.position);
            Vector3 targetWorldPos = cam.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, cam.nearClipPlane + 5.0f));

            
            float curveHeight = 2.0f;
            Vector3 midPos = Vector3.Lerp(startPos, targetWorldPos, 0.5f) + Vector3.up * curveHeight;

            
            Vector3 m1 = Vector3.Lerp(startPos, midPos, t);
            Vector3 m2 = Vector3.Lerp(midPos, targetWorldPos, t);
            obj.transform.position = Vector3.Lerp(m1, m2, t);

            
            obj.transform.localScale = Vector3.one * (1f - t);

            yield return null;
        }

        Destroy(obj);
        onComplete?.Invoke();
    }
}

