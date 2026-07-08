using UnityEngine;

namespace Babel
{
    public class Cloud : MonoBehaviour
    {
        [Header("구름 스프라이트")]
        [SerializeField] private Sprite[] sprites;

        [Header("구름 크기")]
        [SerializeField] private float minScale = 0.6f;
        [SerializeField] private float maxScale = 1.0f;

        private SpriteRenderer spriteRenderer;
        private float speedWeight = 1.0f;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnEnable()
        {
            if (sprites != null && sprites.Length > 0)
            {
                int randomSpriteIndex = Random.Range(0, sprites.Length);
                spriteRenderer.sprite = sprites[randomSpriteIndex];
            }

            float randomScale = Random.Range(minScale, maxScale);
            transform.localScale = Vector3.one * randomScale;
            speedWeight = 1.0f / randomScale;
        }

        public void Move(Vector3 flowSpeed)
        {
            transform.Translate(flowSpeed * speedWeight);
        }
    }
}
