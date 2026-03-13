using System.Collections;
using UnityEngine;

public class UIAbsorber : SingletonBase<UIAbsorber>
{
    [Header("Target UI Transforms")]
    [SerializeField] private RectTransform goldTargetUI; // 왼쪽 상단 골드 아이콘
    [SerializeField] private RectTransform chestTargetUI; // 오른쪽 상단 상자 아이콘

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

        // 월드 좌표를 스크린 좌표로 변환 후 UI 타겟 지점 계산
        // (단순화를 위해 매 프레임 타겟의 스크린 위치를 월드 좌표로 역산하여 추적)
        
        while (elapsed < duration)
        {
            if (obj == null) yield break;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 1. 타겟 UI의 현재 월드 위치 계산 (카메라 거리에 맞춰)
            Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(null, targetUI.position);
            Vector3 targetWorldPos = cam.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, cam.nearClipPlane + 5.0f));

            // 2. 곡선 연출을 위한 중간 지점 (Bezier 느낌)
            float curveHeight = 2.0f;
            Vector3 midPos = Vector3.Lerp(startPos, targetWorldPos, 0.5f) + Vector3.up * curveHeight;

            // 3. 2차 베지어 곡선 보간
            Vector3 m1 = Vector3.Lerp(startPos, midPos, t);
            Vector3 m2 = Vector3.Lerp(midPos, targetWorldPos, t);
            obj.transform.position = Vector3.Lerp(m1, m2, t);

            // 4. 크기 축소 연출
            obj.transform.localScale = Vector3.one * (1f - t);

            yield return null;
        }

        Destroy(obj);
        onComplete?.Invoke();
    }
}
