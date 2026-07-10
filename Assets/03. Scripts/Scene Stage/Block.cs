using UnityEngine;

namespace Babel
{
    public class Block : MonoBehaviour
    {
        [Header("블록 기본 설정")]
        [SerializeField] protected int blockValue = 100;

        [Header("회전 설정")]
        [SerializeField] private float minRotationSpeed = 5f;
        [SerializeField] private float maxRotationSpeed = 30f;

        [Header("바람 설정")]
        [SerializeField] protected float windPower = 0.35f;

        protected Rigidbody2D Rb;
        protected Collider2D Col;
        private float baseMass;
        private float assignedAngularVelocity;
        private float assignedWindSpeed;
        private bool isUnderWaterBlock;
        protected bool IsFired;

        public bool HasFirstContact { get; private set; }
        public bool HasLost { get; private set; }
        public float WindPower => windPower;

        protected virtual void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            Col = GetComponent<Collider2D>();
            Col.enabled = false;
            Rb.gravityScale = 0.0f;
            Rb.bodyType = RigidbodyType2D.Kinematic;
            baseMass = Rb.mass;
            float randomSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
            float randomDirection = Random.Range(-1, 1) >= 0 ? 1f : -1f;
            assignedAngularVelocity = randomSpeed * randomDirection;
        }

        protected virtual void Update()
        {
            if (GameManager.IsInitialized && GameManager.Instance.IsGameOver)
            {
                return;
            }

            IsUnderWater();

            if (IsFired == false)
            {
                transform.Rotate(Vector3.forward * assignedAngularVelocity * Time.deltaTime);
            }
        }

        protected virtual void FixedUpdate()
        {
            if (GameManager.IsInitialized && GameManager.Instance.IsGameOver)
            {
                return;
            }

            ApplyWind();
        }

        protected virtual void OnEnable()
        {
            float randomScale = Random.Range(0.8f, 1.2f);
            transform.localScale *= randomScale;

            if (Rb != null)
            {
                Rb.mass = baseMass * randomScale;
            }
        }

        public virtual void Launch(Vector2 force)
        {
            Launch(force, 0.0f);
        }

        public virtual void Launch(Vector2 force, float windSpeed)
        {
            IsFired = true;
            assignedWindSpeed = windSpeed;
            Col.enabled = true;
            Rb.bodyType = RigidbodyType2D.Dynamic;
            Rb.gravityScale = 1.0f;
            transform.SetParent(null);
            Rb.AddForce(force, ForceMode2D.Impulse);
            Rb.angularVelocity = assignedAngularVelocity;
        }

        protected virtual void ApplyWind()
        {
            if (IsFired == false) return;
            if (Rb.bodyType != RigidbodyType2D.Dynamic) return;
            if (HasFirstContact) return;
            if (HasLost) return;
            if (isUnderWaterBlock) return;

            Vector2 windForce = Vector2.right * assignedWindSpeed * windPower * Rb.mass;
            Rb.AddForce(windForce, ForceMode2D.Force);
        }

        protected virtual void IsUnderWater()
        {
            if (transform.position.y <= -0.5f && isUnderWaterBlock == false)
            {
                isUnderWaterBlock = true;
                Rb.gravityScale /= 5;
                Rb.linearVelocity /= 500;
                Rb.angularVelocity /= 10;
            }
            else if (transform.position.y > -0.5f && isUnderWaterBlock)
            {
                isUnderWaterBlock = false;
                Rb.gravityScale *= 5;
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (GameManager.Instance.IsGameOver) return;
            if (HasLost) return;

            if (HasFirstContact == false)
            {
                HasFirstContact = true;
                OnFirstContact(collision);
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (GameManager.Instance.IsGameOver) return;

            if (collision.CompareTag("Underwater") && HasLost == false)
            {
                HasLost = true;
                OnLost(collision);
            }
        }

        protected virtual void OnFirstContact(Collision2D collision)
        {
            GameManager.Instance.AddLoot(blockValue);
        }

        protected virtual void OnLost(Collider2D collision)
        {
            GameManager.Instance.LostLoot(blockValue);
        }
    }
}
