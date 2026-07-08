using System.Collections;
using UnityEngine;

namespace Babel
{
    public class BombBlock : Block
    {
        [Header("폭탄 대기 시간")]
        [SerializeField] private float minIdleTime = 2.0f;
        [SerializeField] private float maxIdleTime = 5.0f;
        [SerializeField] private bool waitUntilAlmostStopped = true;
        [SerializeField] private float restingVelocity = 0.25f;
        [SerializeField] private float restingAngularVelocity = 15.0f;
        [SerializeField] private float requiredRestingTime = 0.35f;

        [Header("폭탄 경고 연출")]
        [SerializeField] private float warningDuration = 1.2f;
        [SerializeField] private Color hotColor = Color.red;
        [SerializeField] private Color explodedColor = new Color(0.15f, 0.15f, 0.15f, 1.0f);

        [Header("폭탄 흔들림")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float shakeAmount = 0.08f;
        [SerializeField] private float shakeFrequency = 45.0f;
        [SerializeField] private bool shakeGetsStronger = true;

        [Header("폭발 설정")]
        [SerializeField] private float explosionRadius = 4.0f;
        [SerializeField] private float explosionForce = 35.0f;
        [SerializeField] private float minimumForceRatio = 0.25f;
        [SerializeField] private float explosionTorque = 20.0f;
        [SerializeField] private LayerMask explosionTargetMask;

        [Header("치킨 보너스")]
        [SerializeField] private int friedChickenBonusScore = 300;

        [Header("폭발 이펙트")]
        [SerializeField] private GameObject explosionEffectPrefab;

        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        private Vector3 visualRootOriginLocalPosition;
        private bool fuseStarted;
        private bool hasExploded;

        protected override void Awake()
        {
            base.Awake();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }

            if (visualRoot == null && spriteRenderer != null && spriteRenderer.transform != transform)
            {
                visualRoot = spriteRenderer.transform;
            }

            if (visualRoot != null)
            {
                visualRootOriginLocalPosition = visualRoot.localPosition;
            }
        }

        protected override void OnFirstContact(Collision2D collision)
        {
            base.OnFirstContact(collision);
            if (isFired == false) return;
            if (fuseStarted) return;
            if (hasExploded) return;

            fuseStarted = true;
            StartCoroutine(BombRoutine());
        }

        private IEnumerator BombRoutine()
        {
            if (isFired == false) yield break;
            if (GameManager.Instance.IsGameOver) yield break;
            if (HasLost) yield break;

            if (waitUntilAlmostStopped)
            {
                yield return WaitUntilAlmostStopped();
            }

            if (isFired == false) yield break;
            if (GameManager.Instance.IsGameOver) yield break;
            if (HasLost) yield break;

            float idleTime = Random.Range(minIdleTime, maxIdleTime);
            yield return new WaitForSeconds(idleTime);

            if (isFired == false) yield break;
            if (GameManager.Instance.IsGameOver) yield break;
            if (HasLost) yield break;

            yield return WarningRoutine();
            Explode();
        }

        private IEnumerator WaitUntilAlmostStopped()
        {
            float restingTimer = 0.0f;

            while (restingTimer < requiredRestingTime)
            {
                if (isFired == false) yield break;
                if (GameManager.Instance.IsGameOver) yield break;
                if (HasLost) yield break;

                bool isAlmostStopped = rb.linearVelocity.sqrMagnitude <= restingVelocity * restingVelocity && Mathf.Abs(rb.angularVelocity) <= restingAngularVelocity;

                if (isAlmostStopped)
                {
                    restingTimer += Time.deltaTime;
                }
                else
                {
                    restingTimer = 0.0f;
                }

                yield return null;
            }
        }

        private IEnumerator WarningRoutine()
        {
            float timer = 0.0f;

            if (visualRoot != null)
            {
                visualRootOriginLocalPosition = visualRoot.localPosition;
            }

            while (timer < warningDuration)
            {
                if (isFired == false) yield break;
                if (GameManager.Instance.IsGameOver) yield break;
                if (HasLost) yield break;

                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / warningDuration);
                float pulse = Mathf.PingPong(timer * 8.0f, 1.0f);

                if (spriteRenderer != null)
                {
                    Color targetColor = Color.Lerp(originalColor, hotColor, Mathf.Lerp(t, 1.0f, pulse));
                    spriteRenderer.color = targetColor;
                }

                ShakeVisual(t);
                yield return null;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = hotColor;
            }

            ResetVisualPosition();
        }

        private void ShakeVisual(float warningProgress)
        {
            if (visualRoot == null) return;

            float currentShakeAmount = shakeAmount;

            if (shakeGetsStronger)
            {
                currentShakeAmount *= Mathf.Lerp(0.25f, 1.0f, warningProgress);
            }

            float x = Mathf.Sin(Time.time * shakeFrequency) * currentShakeAmount;
            float y = Mathf.Cos(Time.time * shakeFrequency * 1.37f) * currentShakeAmount;
            visualRoot.localPosition = visualRootOriginLocalPosition + new Vector3(x, y, 0.0f);
        }

        private void ResetVisualPosition()
        {
            if (visualRoot != null)
            {
                visualRoot.localPosition = visualRootOriginLocalPosition;
            }
        }

        private void Explode()
        {
            if (hasExploded) return;

            hasExploded = true;
            ResetVisualPosition();

            if (spriteRenderer != null)
            {
                spriteRenderer.color = explodedColor;
            }

            if (explosionEffectPrefab != null)
            {
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            }

            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, explosionRadius, explosionTargetMask);

            foreach (Collider2D target in targets)
            {
                if (target == null) continue;
                if (target.transform == transform) continue;

                Rigidbody2D targetRigidbody = target.attachedRigidbody;

                if (targetRigidbody != null)
                {
                    Vector2 direction = targetRigidbody.worldCenterOfMass - (Vector2)transform.position;
                    float distance = Mathf.Max(direction.magnitude, 0.1f);
                    float forceRatio = Mathf.Clamp01(1.0f - distance / explosionRadius);
                    forceRatio = Mathf.Max(forceRatio, minimumForceRatio);
                    Vector2 forceDirection = direction.normalized;
                    targetRigidbody.AddForce(forceDirection * explosionForce * forceRatio, ForceMode2D.Impulse);
                    targetRigidbody.AddTorque(Random.Range(-explosionTorque, explosionTorque) * forceRatio, ForceMode2D.Impulse);
                }

                ChickenBlock chickenBlock = target.GetComponentInParent<ChickenBlock>();
                if (chickenBlock != null)
                {
                    chickenBlock.Fry(friedChickenBonusScore);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
