using UnityEngine;

namespace Babel
{

    public class Ship : MonoBehaviour
    {
        public Rigidbody2D rb;

        [Header("부력 피벗")]
        public Transform leftBuoyancyPoint;
        public Transform rightBuoyancyPoint;

        [Header("부력 값")]
        public float baseBuoyancy = 15f; // 기본적으로 떠받치는 힘
        public float restoreFactor = 5f; // 기울어졌을 때 복원하는 힘의 계수

        [Header("해수면 설정")]
        public float waterLevel = 0.0f;
        public float waterResist = 2.0f;

        private void FixedUpdate()
        {
            ApplyPointBuoyancy(leftBuoyancyPoint);
            ApplyPointBuoyancy(rightBuoyancyPoint);
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

