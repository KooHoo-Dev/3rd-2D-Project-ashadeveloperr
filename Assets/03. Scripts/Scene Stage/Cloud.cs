using Unity.VisualScripting;
using UnityEngine;

namespace Babel
{

    public class Cloud : MonoBehaviour
    {
        [SerializeField] private float minScale = 0.6f;
        [SerializeField] private float maxScale = 1.0f;

        SpriteRenderer spriteRenderer;

        public Sprite[] sprites;

        private float speedWeight = 1.0f; // 구름 속도 랜덤 가중치임

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
