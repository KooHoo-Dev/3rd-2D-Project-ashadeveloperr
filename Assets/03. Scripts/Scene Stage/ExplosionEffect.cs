using System.Collections;
using UnityEngine;

namespace Babel
{
    public class ExplosionEffect : MonoBehaviour
    {
        [Header("Scale Settings")]
        [SerializeField] private float startScale = 0.2f;
        [SerializeField] private float maxScale = 4.0f;
        [SerializeField] private float duration = 0.18f;

        [Header("Fade Settings")]
        [SerializeField] private float startAlpha = 1.0f;
        [SerializeField] private float endAlpha = 0.0f;

        private SpriteRenderer spriteRenderer;
        private Color originalColor;

        private void Awake()
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        private void OnEnable()
        {
            StartCoroutine(EffectRoutine());
        }

        private IEnumerator EffectRoutine()
        {
            transform.localScale = Vector3.one * startScale;

            if (spriteRenderer != null)
            {
                Color color = originalColor;
                color.a = startAlpha;
                spriteRenderer.color = color;
            }

            float timer = 0.0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / duration);

                float scale = Mathf.Lerp(startScale, maxScale, t);
                transform.localScale = Vector3.one * scale;

                if (spriteRenderer != null)
                {
                    Color color = originalColor;
                    color.a = Mathf.Lerp(startAlpha, endAlpha, t);
                    spriteRenderer.color = color;
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}