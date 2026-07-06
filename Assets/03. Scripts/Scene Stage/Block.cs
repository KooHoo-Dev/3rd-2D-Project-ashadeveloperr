using UnityEngine;

namespace Babel
{
    public class Block : MonoBehaviour
    {
        public int level = 1;
        public bool HasFirstContact { get; private set; } = false;
        public bool HasLost { get; private set; } = false;

        protected float baseMass;

        [Header("Rotation Settings")]
        public float minRotationSpeed = 5f;
        public float maxRotationSpeed = 30f;
        protected float assignedAngularVelocity;
        protected bool isFired = false;

        [Header("Wind Settings")]
        [SerializeField] protected float windPower = 0.35f;
        public float WindPower => windPower;

        protected float assignedWindSpeed;

        protected bool isUnderWater;
        public int blockValue = 100;

        protected Rigidbody2D rb;
        protected Collider2D col;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();

            col.enabled = false;
            rb.gravityScale = 0.0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            baseMass = rb.mass;

            float randomSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
            float randomDirection = Random.Range(-1, 1) >= 0 ? 1f : -1f;
            assignedAngularVelocity = randomSpeed * randomDirection;
        }

        protected virtual void Update()
        {
            IsUnderWater();

            if (isFired == false)
            {
                transform.Rotate(Vector3.forward * assignedAngularVelocity * Time.deltaTime);
            }
        }

        protected virtual void FixedUpdate()
        {
            ApplyWind();
        }

        protected virtual void OnEnable()
        {
            float randomScale = Random.Range(0.8f, 1.2f);
            transform.localScale *= randomScale;

            if (rb != null)
            {
                rb.mass = baseMass * randomScale;
            }
        }

        public virtual void Launch(Vector2 force)
        {
            Launch(force, 0.0f);
        }

        public virtual void Launch(Vector2 force, float windSpeed)
        {
            isFired = true;
            assignedWindSpeed = windSpeed;

            col.enabled = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1.0f;

            transform.SetParent(null);

            rb.AddForce(force, ForceMode2D.Impulse);
            rb.angularVelocity = assignedAngularVelocity;
        }

        protected virtual void ApplyWind()
        {
            if (isFired == false) return;
            if (rb.bodyType != RigidbodyType2D.Dynamic) return;
            if (HasFirstContact) return;
            if (HasLost) return;
            if (isUnderWater) return;

            Vector2 windForce = Vector2.right * assignedWindSpeed * windPower * rb.mass;
            rb.AddForce(windForce, ForceMode2D.Force);
        }

        protected virtual void IsUnderWater()
        {
            if (transform.position.y <= -0.5f && isUnderWater == false)
            {
                isUnderWater = true;
                rb.gravityScale /= 5;
                rb.linearVelocity /= 500;
                rb.angularVelocity /= 10;
            }
            else if (transform.position.y > -0.5f && isUnderWater)
            {
                isUnderWater = false;
                rb.gravityScale *= 5;
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (GameManager.Instance.isGameOver) return;
            if (HasLost) return;

            if (HasFirstContact == false)
            {
                HasFirstContact = true;
                OnFirstContact(collision);
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (GameManager.Instance.isGameOver) return;

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