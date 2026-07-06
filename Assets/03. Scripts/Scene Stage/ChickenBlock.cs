using UnityEngine;

namespace Babel
{
    public class ChickenBlock : Block
    {
        [Header("Fried Chicken Settings")]
        [SerializeField] private Sprite friedSprite;
        [SerializeField] private PhysicsMaterial2D friedPhysicsMaterial;
        [SerializeField] private Color friedColor = new Color(1.0f, 0.65f, 0.2f, 1.0f);

        private SpriteRenderer spriteRenderer;
        private bool isFried;

        protected override void Awake()
        {
            base.Awake();

            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        public void Fry(int bonusScore)
        {
            if (isFried) return;
            if (HasLost) return;

            isFried = true;

            if (spriteRenderer != null)
            {
                if (friedSprite != null)
                {
                    spriteRenderer.sprite = friedSprite;
                }
                else
                {
                    spriteRenderer.color = friedColor;
                }
            }

            if (friedPhysicsMaterial != null && col != null)
            {
                col.sharedMaterial = friedPhysicsMaterial;
            }

            blockValue += bonusScore;

            if (HasFirstContact)
            {
                GameManager.Instance.AddLoot(bonusScore);
            }
        }
    }
}