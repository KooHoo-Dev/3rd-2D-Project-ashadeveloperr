using UnityEngine;

namespace Babel
{
    public class Block : MonoBehaviour
    {
        public int level = 1;
        public bool HasFirstContact { get; private set; } = false;
        public bool HasLost { get; private set; } = false;

        [Header("Rotation Settings")]
        public float minRotationSpeed = 5f; // 최소 회전 속도
        public float maxRotationSpeed = 30f; // 최대 회전 속도
        private float assignedAngularVelocity; // 이 블록에 부여된 랜덤 회전 속도
        private bool isFired = false;          // 발사 여부 체크

        private bool isUnderWater;
        private int blockValue = 100;

        private Rigidbody2D rb;
        private Collider2D col;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            col.enabled = false;
            rb.gravityScale = 0.0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            // ⭐ [추가] 생성되자마자 이 블록이 가질 고유한 회전 속도와 방향(양수/음수)을 무작위로 결정
            float randomSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
            float randomDirection = Random.Range(-1, 1) >= 0 ? 1f : -1f;
            assignedAngularVelocity = randomSpeed * randomDirection;
        }

        private void Update()
        {
            IsUnderWater();

            // ⭐ [추가] 발사되기 전(Kinematic 상태)에는 트랜스폼을 이용해 시각적으로 돌려줍니다.
            if (!isFired)
            {
                transform.Rotate(Vector3.forward * assignedAngularVelocity * Time.deltaTime);
            }
        }

        // ⭐ [수정] PlayerController가 호출하던 Launch 함수에 물리 회전 추가
        public void Launch(Vector2 force)
        {
            isFired = true; // 대기 상태 회전 루틴 해제

            col.enabled = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1.0f;

            transform.SetParent(null);

            // 물리 힘(Impulse) 적용
            rb.AddForce(force, ForceMode2D.Impulse);

            // ⭐ [추가] 대기할 때 돌던 속도 그대로 물리 회전 속도(angularVelocity)로 이어받음
            rb.angularVelocity = assignedAngularVelocity;
        }

        private void IsUnderWater()
        {
            if (transform.position.y <= -0.5f && isUnderWater == false)
            {
                isUnderWater = true;
                rb.gravityScale /= 5;
                rb.linearVelocity /= 500;
                rb.angularVelocity /= 10; // 물에 들어가면 회전도 감속되도록 추가하면 자연스럽습니다.
            }
            else if (transform.position.y > -0.5f && isUnderWater)
            {
                isUnderWater = false;
                rb.gravityScale *= 5;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (GameManager.Instance.isGameOver) return;
            if (HasLost) return;

            if (HasFirstContact == false)
            {
                HasFirstContact = true;
                GameManager.Instance.AddLoot(blockValue);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (GameManager.Instance.isGameOver) return;
            if (HasLost) return;

            if (HasLost == false && collision.CompareTag("Underwater"))
            {
                HasLost = true;
                GameManager.Instance.LostLoot(blockValue);
            }
        }
    }
}