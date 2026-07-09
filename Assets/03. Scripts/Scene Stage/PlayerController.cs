using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Babel
{
    [RequireComponent(typeof(LineRenderer))]
    public class PlayerController : MonoBehaviour
    {
        [Header("발사 세기")]
        [SerializeField] private float forceMultiplier = 30.0f;
        [SerializeField] private float maxForce = 150.0f;

        [Header("궤적 표시")]
        [SerializeField] private LineRenderer trajectoryLine;
        [SerializeField] private int trajectoryPointCount = 30;
        [SerializeField] private float timeStepPerPoint = 0.1f;

        [Header("블록 생성 위치")]
        [SerializeField] private Transform spawnPoint;

        private Block currentBlock;
        private Vector2 dragStartPos;
        private bool isDragging;
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;

            if (trajectoryLine == null) trajectoryLine = GetComponent<LineRenderer>();
            trajectoryLine.positionCount = trajectoryPointCount;

            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                trajectoryLine.material = new Material(spriteShader);
            }

            trajectoryLine.startWidth = 0.25f;
            trajectoryLine.endWidth = 0.01f;
            Gradient orangeGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            Color orangeColor = new Color(1.0f, 0.5f, 0.0f);
            Color yellowColor = new Color(1.0f, 1.0f, 0.0f);
            colorKeys[0] = new GradientColorKey(orangeColor, 0.0f);
            colorKeys[1] = new GradientColorKey(yellowColor, 1.0f);
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(0.75f, 0.0f);
            alphaKeys[1] = new GradientAlphaKey(0.0f, 1.0f);
            orangeGradient.SetKeys(colorKeys, alphaKeys);
            trajectoryLine.colorGradient = orangeGradient;
            trajectoryLine.numCornerVertices = 5;
            trajectoryLine.numCapVertices = 5;
            trajectoryLine.enabled = false;
            SpawnBlock();
        }

        private void Update()
        {
            if (UIManager.IsInitialized && UIManager.Instance.IsTutorialSplashActive)
            {
                if (trajectoryLine.enabled)
                {
                    trajectoryLine.enabled = false;
                }

                isDragging = false;
                return;
            }

            if (currentBlock == null)
            {
                if (trajectoryLine.enabled) trajectoryLine.enabled = false;
                isDragging = false;
                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;
                StartDrag();
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (isDragging)
                {
                    FireBlock();
                }
            }
            else if (Mouse.current.leftButton.isPressed && isDragging)
            {
                OnDragging();
            }
        }

        private void StartDrag()
        {
            dragStartPos = GetMouseWorldPosition();
            isDragging = true;
            trajectoryLine.enabled = true;
        }

        private void OnDragging()
        {
            if (isDragging == false) return;

            Rigidbody2D blockRigidbody = currentBlock.GetComponent<Rigidbody2D>();
            forceMultiplier = blockRigidbody.mass / 1.5f;
            maxForce = blockRigidbody.mass * 30;
            Vector2 currentMousePos = GetMouseWorldPosition();
            Vector2 dragVector = dragStartPos - currentMousePos;
            UIManager.Instance.SetCameraTarget(currentBlock.transform, UIManager.Instance.ThrowerOffset, 25f);

            if (dragVector.x < 0)
            {
                dragVector.x = 0;
            }

            if (currentBlock != null)
            {
                currentBlock.transform.localPosition = Vector3.zero;
            }

            Vector2 predictedForce = dragVector * forceMultiplier;
            if (predictedForce.magnitude > maxForce)
            {
                predictedForce = predictedForce.normalized * maxForce;
            }

            float blockMass = currentBlock != null ? currentBlock.GetComponent<Rigidbody2D>().mass : 1.0f;
            Vector2 initialVelocity = predictedForce / blockMass;
            DrawTrajectory(spawnPoint.position, initialVelocity);
        }

        private void FireBlock()
        {
            isDragging = false;
            trajectoryLine.enabled = false;

            if (currentBlock == null) return;

            Vector2 currentMousePos = GetMouseWorldPosition();
            Vector2 dragVector = dragStartPos - currentMousePos;

            if (dragVector.x < 10)
            {
                dragVector.x = 10;
            }

            Vector2 shootForce = dragVector * forceMultiplier;

            if (shootForce.magnitude > maxForce)
            {
                shootForce = shootForce.normalized * maxForce;
            }

            currentBlock.Launch(shootForce, GameManager.Instance.CurrentWindSpeed);
            UIManager.Instance.SetCameraTarget(currentBlock.transform, UIManager.Instance.BlockOffset, 18f);
            StartCoroutine(SpawnDelayCoroutine(currentBlock));
            currentBlock = null;
        }

        private void SpawnBlock()
        {
            if (currentBlock != null) return;

            Block spawnBlock = GameManager.Instance.GetNextBlock();
            currentBlock = Instantiate(spawnBlock, spawnPoint);
            currentBlock.transform.localPosition = Vector3.zero;
        }

        private IEnumerator SpawnDelayCoroutine(Block dropBlock)
        {
            yield return new WaitForSeconds(0.5f);

            while (true)
            {
                if (dropBlock == null || dropBlock.HasFirstContact || dropBlock.HasLost) break;
                yield return null;
            }

            UIManager.Instance.SetCameraTarget(UIManager.Instance.CurrentShip.transform, UIManager.Instance.ShipPosOffset, 20f);

            if (GameManager.Instance.IsGameOver == false)
            {
                GameManager.Instance.ChangeWindForNextThrow();
            }

            SpawnBlock();
        }

        private void DrawTrajectory(Vector2 startPos, Vector2 velocity)
        {
            Vector3[] points = new Vector3[trajectoryPointCount];
            Vector2 gravity = Physics2D.gravity;
            float windPower = currentBlock != null ? currentBlock.WindPower : 0.0f;
            Vector2 windAcceleration = Vector2.right * GameManager.Instance.CurrentWindSpeed * windPower;

            for (int i = 0; i < trajectoryPointCount; i++)
            {
                float time = i * timeStepPerPoint;
                Vector2 totalAcceleration = gravity + windAcceleration;
                Vector2 pointPos = startPos + (velocity * time) + (0.5f * totalAcceleration * (time * time));
                points[i] = new Vector3(pointPos.x, pointPos.y, 0f);
            }

            trajectoryLine.SetPositions(points);
        }

        private Vector2 GetMouseWorldPosition()
        {
            Vector2 mouseScreenPos = Mouse.current.position.value;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -mainCamera.transform.position.z));
            return new Vector2(worldPos.x, worldPos.y);
        }
    }
}
