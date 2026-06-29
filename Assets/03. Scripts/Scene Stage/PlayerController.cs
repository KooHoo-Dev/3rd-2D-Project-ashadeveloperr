using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Babel
{
    [RequireComponent(typeof(LineRenderer))]
    public class PlayerController : MonoBehaviour
    {
        #region Settings

        //[Header("Sling Settings (시각적 피드백)")]
        //public float maxDragDistance = 2.0f; // 마우스로 당길 수 있는 최대 거리
        //public float visualPullbackStrength = 0.5f; // 실제 드래그 거리 대비 오브젝트가 뒤로 밀리는 비율

        [Header("Physics Settings (발사 세기)")]
        public float forceMultiplier = 3.0f;
        public float maxForce = 150.0f;

        [Header("Trajectory Settings (궤적 포물선)")]
        public LineRenderer trajectoryLine;
        public int trajectoryPointCount = 30;
        public float timeStepPerPoint = 0.1f;

        [Header("Ref")]
        public Transform spawnPoint; // 블록이 대기하는 기본 위치
        private Block currentBlock;

        // 드래그 제어 변수
        private Vector2 dragStartPos; // 마우스 클릭 시작 월드 좌표
        private bool isDragging = false;
        private Camera mainCamera;

        #endregion

        #region Unity Methods

        private void Start()
        {
            mainCamera = Camera.main;

            if (trajectoryLine == null) trajectoryLine = GetComponent<LineRenderer>();
            trajectoryLine.positionCount = trajectoryPointCount;

            // --- 궤적 스타일 자동 설정 (오렌지색 & 얇게) ---

            // ⭐ [핵심 추가] 코드로 유니티 기본 스프라이트 머티리얼을 찾아서 라인렌더러에 강제 주입!
            // 이 셰이더/머티리얼이 들어가야 오렌지색과 투명도(알파값)가 정상 작동합니다.
            Shader spriteShader = Shader.Find("Sprites/Default");
            if (spriteShader != null)
            {
                trajectoryLine.material = new Material(spriteShader);
            }

            // 1. 궤적 굵기 대폭 축소 (시작 두께 0.2, 끝 두께 0.01로 점점 얇아지게)
            trajectoryLine.startWidth = 0.2f;
            trajectoryLine.endWidth = 0.01f;

            // 2. 부드러운 오렌지색 -> 투명 그라데이션 코드 자동 생성
            Gradient orangeGradient = new Gradient();

            // 색상 키: 시작(오렌지) -> 끝(오렌지)
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            Color orangeColor = new Color(1.0f, 0.5f, 0.0f); // RGB 오렌지색
            colorKeys[0] = new GradientColorKey(orangeColor, 0.0f);
            colorKeys[1] = new GradientColorKey(orangeColor, 1.0f);

            // 알파(투명도) 키: 시작(불투명) -> 끝(완전 투명)
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f); // 0% 지점: 선명함
            alphaKeys[1] = new GradientAlphaKey(0.0f, 1.0f); // 100% 지점: 투명함

            orangeGradient.SetKeys(colorKeys, alphaKeys);
            trajectoryLine.colorGradient = orangeGradient;

            // 3. 선이 각지지 않고 둥글게 표현되도록 설정
            trajectoryLine.numCornerVertices = 5;
            trajectoryLine.numCapVertices = 5;

            trajectoryLine.enabled = false;

            SpawnBlock();
        }

        private void Update()
        {
            // ⭐ [추가] 조준하거나 발사할 블록이 없다면 마우스 입력을 전부 무시합니다.
            if (currentBlock == null)
            {
                // 혹시 마우스를 뗐을 때 궤적이 남아있지 않도록 꺼주는 안전장치
                if (trajectoryLine.enabled) trajectoryLine.enabled = false;
                isDragging = false;
                return;
            }

            // 1. 마우스 클릭 시 (드래그 시작)
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                StartDrag();
            }
            // 2. 마우스 버튼을 뗄 때 (발사)
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (isDragging)
                {
                    FireBlock();
                }
            }
            // 3. 드래그 중일 때
            else if (Mouse.current.leftButton.isPressed && isDragging)
            {
                OnDragging();
            }
        }

        #endregion

        #region Custom Methods

        private void StartDrag()
        {
            dragStartPos = GetMouseWorldPosition();
            isDragging = true;
            trajectoryLine.enabled = true; // 조준 시작 시 궤적 켬
        }

        private void OnDragging()
        {
            Vector2 currentMousePos = GetMouseWorldPosition();
            Vector2 dragVector = dragStartPos - currentMousePos;

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

            float blockMass = (currentBlock != null) ? currentBlock.GetComponent<Rigidbody2D>().mass : 1.0f;
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

            // ⭐ [수정] 발사할 때도 마찬가지로 왼쪽(뒤) 발사를 차단합니다.
            if (dragVector.x < 10)
            {
                dragVector.x = 10;
            }

            // 최종 날아갈 방향과 크기 계산
            Vector2 shootForce = dragVector * forceMultiplier;

            if (shootForce.magnitude > maxForce)
            {
                shootForce = shootForce.normalized * maxForce;
            }

            // 블록 발사!
            currentBlock.Launch(shootForce);

            UIManager.Instance.SetCameraTarget(currentBlock.transform, UIManager.Instance.blockOffset);

            StartCoroutine(SpawnDelayCoroutine(currentBlock));
            currentBlock = null;
        }

        private void SpawnBlock()
        {
            // 아직 이전 블록 처리가 안 끝났으면 리턴
            if (currentBlock != null) return;

            Block spawnBlock = GameManager.Instance.GetNextBlock();
            // spawnPoint의 자식으로 생성
            currentBlock = Instantiate(spawnBlock, spawnPoint);
            // 위치 초기화 (spawnPoint 위치에 겹침)
            currentBlock.transform.localPosition = Vector3.zero;
        }

        private IEnumerator SpawnDelayCoroutine(Block dropBlock)
        {
            // 새 블록 스폰 전 약간의 대기 시간을 주어 발사 연출을 감상하게 할 수 있습니다.
            yield return new WaitForSeconds(0.5f);

            while (true)
            {
                // 날아간 블록이 파괴되었거나 충돌했다거나 물에 빠졌다면 루프 탈출
                if (dropBlock == null || dropBlock.HasFirstContact || dropBlock.HasLost) break;
                yield return null;
            }

            UIManager.Instance.SetCameraTarget(UIManager.Instance.ship.transform, UIManager.Instance.shipPosOffset);

            SpawnBlock();
        }

        // --- 궤적 계산 핵심 함수 ---
        private void DrawTrajectory(Vector2 startPos, Vector2 velocity)
        {
            Vector3[] points = new Vector3[trajectoryPointCount];

            // 물리 공식: 위치 = 초기위치 + 초기속도*시간 + 0.5*중력*시간^2
            Vector2 gravity = Physics2D.gravity;

            for (int i = 0; i < trajectoryPointCount; i++)
            {
                float time = i * timeStepPerPoint;

                // 포물선 공식 적용
                Vector2 pointPos = startPos + (velocity * time) + (0.5f * gravity * (time * time));

                points[i] = new Vector3(pointPos.x, pointPos.y, 0f); // 2D이므로 Z는 0
            }

            trajectoryLine.SetPositions(points);
        }

        // 마우스 스크린 좌표를 2D 월드 좌표(X, Y)로 변환해주는 헬퍼
        private Vector2 GetMouseWorldPosition()
        {
            Vector2 mouseScreenPos = Mouse.current.position.value;
            // 카메라 z값만큼 depth를 주어 정확한 월드 XY좌표를 얻음
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, -mainCamera.transform.position.z));
            return new Vector2(worldPos.x, worldPos.y);
        }

        #endregion
    }
}