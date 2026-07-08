using UnityEngine;
using UnityEngine.Serialization;

namespace Babel
{
    public class Ship : MonoBehaviour
    {
        [Header("물리 참조")]
        [FormerlySerializedAs("rb")]
        [SerializeField] private Rigidbody2D rb;

        [Header("갑판 피벗")]
        [FormerlySerializedAs("deckLeft")]
        [SerializeField] private Transform deckLeft;
        [FormerlySerializedAs("deckRight")]
        [SerializeField] private Transform deckRight;
        [FormerlySerializedAs("deckCenter")]
        [SerializeField] private Transform deckCenter;

        [Header("부력 피벗")]
        [FormerlySerializedAs("leftBuoyancyPoint")]
        [SerializeField] private Transform leftBuoyancyPoint;
        [FormerlySerializedAs("rightBuoyancyPoint")]
        [SerializeField] private Transform rightBuoyancyPoint;

        [Header("해수면 설정")]
        [SerializeField] private float waterLevel = 0.0f;
        [SerializeField] private float waterResist = 2.0f;

        private float baseBuoyancy;

        public Rigidbody2D Rigidbody => rb;
        public Transform DeckLeft => deckLeft;
        public Transform DeckRight => deckRight;
        public Transform DeckCenter => deckCenter;

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = rb.mass / 5000;
            baseBuoyancy = rb.mass * 0.55f;
        }

        private void FixedUpdate()
        {
            ApplyPointBuoyancy(leftBuoyancyPoint);
            ApplyPointBuoyancy(rightBuoyancyPoint);
            UpdateGravityBasedOnDepth();

            if (GameManager.Instance.IsGameOver && GameManager.Instance.IsFailed == false)
            {
                rb.constraints = RigidbodyConstraints2D.FreezePositionY;
            }
        }

        private void UpdateGravityBasedOnDepth()
        {
            if (GameManager.Instance.IsGameOver) return;

            float shipFlying = transform.position.y;

            if (shipFlying > 0)
            {
                float t = Mathf.Clamp01(shipFlying / 2.5f);
                rb.gravityScale = Mathf.Lerp(0.1f, 0.5f, t);
            }
            else
            {
                rb.gravityScale = 0.1f;
            }
        }

        private void ApplyPointBuoyancy(Transform buoyancyPoint)
        {
            float depth = waterLevel - buoyancyPoint.position.y;

            if (depth > 0)
            {
                float buoyancyForce = depth * baseBuoyancy;
                Vector2 velocityAtPoint = rb.GetPointVelocity(buoyancyPoint.position);
                float dampingForce = velocityAtPoint.y * -waterResist;
                float totalVerticalForce = Mathf.Max(0, buoyancyForce + dampingForce);
                rb.AddForceAtPosition(Vector2.up * totalVerticalForce, buoyancyPoint.position);
            }
        }
    }
}
