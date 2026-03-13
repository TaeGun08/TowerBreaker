using System.Collections;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMesh;
    [SerializeField] private float moveSpeed = 100f; // UI는 월드보다 단위가 크므로 속도 상향
    [SerializeField] private float lifetime = 0.8f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color critColor = Color.yellow;

    public void Setup(int damage, bool isCrit)
    {
        if (_textMesh == null) _textMesh = GetComponent<TextMeshProUGUI>();
        
        _textMesh.text = damage.ToString();
        _textMesh.color = isCrit ? critColor : normalColor;
        _textMesh.fontSize = isCrit ? 60 : 40; // UI 폰트 크기 조정

        StartCoroutine(AnimateSequence());
    }

    private IEnumerator AnimateSequence()
    {
        float elapsed = 0f;
        Color startColor = _textMesh.color;
        RectTransform rect = transform as RectTransform;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            // UI 위쪽 방향으로 이동
            if (rect != null)
            {
                rect.anchoredPosition += Vector2.up * (moveSpeed * Time.deltaTime);
            }

            // 서서히 투명해짐
            _textMesh.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

            yield return null;
        }

        Destroy(gameObject);
    }
}
