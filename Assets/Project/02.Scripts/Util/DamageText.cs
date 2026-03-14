using System.Collections;
using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textMesh;
    [SerializeField] private float moveSpeed = 100f; 
    [SerializeField] private float lifetime = 0.8f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color critColor = Color.yellow;

    public void Setup(int damage, bool isCrit)
    {
        if (_textMesh == null) _textMesh = GetComponent<TextMeshProUGUI>();
        
        _textMesh.text = damage.ToString();
        _textMesh.color = isCrit ? critColor : normalColor;
        _textMesh.fontSize = isCrit ? 60 : 40; 

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

            
            if (rect != null)
            {
                rect.anchoredPosition += Vector2.up * (moveSpeed * Time.deltaTime);
            }

            
            _textMesh.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

            yield return null;
        }

        Destroy(gameObject);
    }
}

