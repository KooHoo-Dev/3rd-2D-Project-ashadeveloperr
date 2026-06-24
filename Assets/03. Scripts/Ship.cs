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

        void FixedUpdate()
        {
            // 1. 현재 배의 Z축 회전각을 확인 (-180 ~ 180도 범위로 보정)
            float currentAngle = Mathf.DeltaAngle(0, transform.eulerAngles.z);

            // 기본 힘에서 기울어진 반대 방향에 힘을 더해줍니다.
            // currentAngle이 양수(왼쪽으로 기묾)이면 왼쪽 힘을 키우고, 음수(오른쪽으로 기묾)이면 오른쪽 힘을 키웁니다.
            float leftForce = baseBuoyancy + (currentAngle * restoreFactor);
            float rightForce = baseBuoyancy - (currentAngle * restoreFactor);

            // 음수 힘이 나오지 않도록 최소값 방어
            leftForce = Mathf.Max(0, leftForce);
            rightForce = Mathf.Max(0, rightForce);

            // 2. 각 하위 객체의 위치에서 '월드 좌표계의 위쪽(Vector2.up)' 방향으로 힘을 가합니다.
            rb.AddForceAtPosition(Vector2.up * leftForce, leftBuoyancyPoint.position);
            rb.AddForceAtPosition(Vector2.up * rightForce, rightBuoyancyPoint.position);
        }
    }

}
