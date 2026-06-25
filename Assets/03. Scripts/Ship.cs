using UnityEngine;

namespace Babel
{

    public class Ship : MonoBehaviour
    {
        public Rigidbody2D rb;

        [Header("갑판 피벗")]
        public Transform deckLeft;
        public Transform deckRight;
        public Transform deckCenter;

        [Header("부력 피벗")]
        public Transform leftBuoyancyPoint;
        public Transform rightBuoyancyPoint;

        [Header("해수면 설정")]
        public float waterLevel = 0.0f;
        public float waterResist = 2.0f;

        private float baseBuoyancy; // 기본적으로 떠받치는 힘
        



        #region Unity Methods


        private void Start()
        {
            rb.mass = transform.localScale.x * transform.localScale.y * 10.0f;
            rb.gravityScale = rb.mass / 10000;
            Debug.Log($"현재 배 무게 {rb.mass}");
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            baseBuoyancy = rb.mass * 0.3f;
        }

        private void FixedUpdate()
        {
            ApplyPointBuoyancy(leftBuoyancyPoint);
            ApplyPointBuoyancy(rightBuoyancyPoint);

        }

        #endregion


        #region Custom Methods

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

        

        #endregion


    }

}

