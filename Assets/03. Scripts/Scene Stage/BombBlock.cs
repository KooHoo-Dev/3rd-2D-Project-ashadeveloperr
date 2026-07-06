using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Babel
{
    public class BombBlock : Block
    {
        [Header("Bomb Timer Settings")]
        [SerializeField] private float minIdleTime = 2.0f;
        [SerializeField] private float maxIdleTime = 5.0f;
        [SerializeField] private bool waitUntilAlmostStopped = true;
        [SerializeField] private float restingVelocity = 0.25f;
        [SerializeField] private float restingAngularVelocity = 15.0f;
        [SerializeField] private float requiredRestingTime = 0.35f;

        [Header("Bomb Warning Settings")]
        [SerializeField] private float warningDuration = 1.2f;
        [SerializeField] private Color hotColor = Color.red;
        [SerializeField] private Color explodedColor = new Color(0.15f, 0.15f, 0.15f, 1.0f);

        [Header("Bomb Shake Settings")]
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float shakeAmount = 0.08f;
        [SerializeField] private float shakeFrequency = 45.0f;
        [SerializeField] private bool shakeGetsStronger = true;

        [Header("Explosion Settings")]
        [SerializeField] private float explosionRadius = 4.0f;
        [SerializeField] private float explosionForce = 35.0f;
        [SerializeField] private float minimumForceRatio = 0.25f;
        [SerializeField] private float explosionTorque = 20.0f;
        [SerializeField] private LayerMask explosionTargetMask;

        [Header("Chicken Bonus Settings")]
        [SerializeField] private int friedChickenBonusScore = 300;

        [Header("Effect")]
        [SerializeField] private GameObject explosionEffectPrefab;

        private SpriteRenderer spriteRenderer;
        private Color originalColor;

        private bool fuseStarted;
        private bool hasExploded;

        private Vector3 visualRootOriginLocalPosition;

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
            if (GameManager.Instance.isGameOver) yield break;
            if (HasLost) yield break;

            if (waitUntilAlmostStopped)
            {
                yield return WaitUntilAlmostStopped();
            }

            if (isFired == false) yield break;
            if (GameManager.Instance.isGameOver) yield break;
            if (HasLost) yield break;

            float idleTime = Random.Range(minIdleTime, maxIdleTime);
            yield return new WaitForSeconds(idleTime);

            if (isFired == false) yield break;
            if (GameManager.Instance.isGameOver) yield break;
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
                if (GameManager.Instance.isGameOver) yield break;
                if (HasLost) yield break;

                bool isAlmostStopped =
                    rb.linearVelocity.sqrMagnitude <= restingVelocity * restingVelocity &&
                    Mathf.Abs(rb.angularVelocity) <= restingAngularVelocity;

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
                if (GameManager.Instance.isGameOver) yield break;
                if (HasLost) yield break;

                timer += Time.deltaTime;

                float t = Mathf.Clamp01(timer / warningDuration);
                float pulse = Mathf.PingPong(timer * 8.0f, 1.0f);

                if (spriteRenderer != null)
                {
                    Color targetColor = Color.Lerp(
                        originalColor,
                        hotColor,
                        Mathf.Lerp(t, 1.0f, pulse)
                    );

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

            float shakeX = Mathf.Sin(Time.time * shakeFrequency) * currentShakeAmount;
            float shakeY = Mathf.Cos(Time.time * shakeFrequency * 1.37f) * currentShakeAmount * 0.5f;

            visualRoot.localPosition = visualRootOriginLocalPosition + new Vector3(shakeX, shakeY, 0.0f);
        }

        private void ResetVisualPosition()
        {
            if (visualRoot == null) return;

            visualRoot.localPosition = visualRootOriginLocalPosition;
        }

        private void Explode()
        {
            if (hasExploded) return;
            if (isFired == false) return;
            if (HasLost) return;
            if (GameManager.Instance.isGameOver) return;

            hasExploded = true;

            ResetVisualPosition();

            if (explosionEffectPrefab != null)
            {
                Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position,
                explosionRadius,
                explosionTargetMask
            );

            HashSet<Rigidbody2D> pushedBodies = new HashSet<Rigidbody2D>();

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];
                if (hit == null) continue;

                Rigidbody2D targetRb = hit.attachedRigidbody;
                if (targetRb == null) continue;
                if (targetRb == rb) continue;
                if (pushedBodies.Contains(targetRb)) continue;

                pushedBodies.Add(targetRb);

                Vector2 direction = targetRb.worldCenterOfMass - (Vector2)transform.position;

                if (direction.sqrMagnitude <= 0.01f)
                {
                    direction = Random.insideUnitCircle.normalized;
                }

                float distance = direction.magnitude;
                float distanceRatio = Mathf.Clamp01(1.0f - distance / explosionRadius);

                float finalForce = explosionForce * Mathf.Lerp(
                    minimumForceRatio,
                    1.0f,
                    distanceRatio
                );

                targetRb.AddForce(direction.normalized * finalForce, ForceMode2D.Impulse);
                targetRb.AddTorque(Random.Range(-explosionTorque, explosionTorque), ForceMode2D.Impulse);

                ChickenBlock chickenBlock = hit.GetComponentInParent<ChickenBlock>();

                if (chickenBlock != null)
                {
                    chickenBlock.Fry(friedChickenBonusScore);
                }
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = explodedColor;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}