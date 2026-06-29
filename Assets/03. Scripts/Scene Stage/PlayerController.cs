using System.Collections;
using System.Collections.Generic; // List 사용을 위해 추가
using UnityEngine;
using UnityEngine.InputSystem;

namespace Babel
{
    [RequireComponent(typeof(LineRenderer))] // 자동으로 LineRenderer가 없으면 추가함
    public class PlayerController : MonoBehaviour
    {
        #region Settings

        [Header("Sling Settings (시각적 피드백)")]
        public float maxDragDistance = 2.0f; // 마우스로 당길 수 있는 최대 거리
        public float visualPullbackStrength = 0.5f; // 실제 드래그 거리 대비 오브젝트가 뒤로 밀리는 비율

        [Header("Physics Settings (발사 세기)")]
        public float forceMultiplier = 5.0f; // 드래그 거리에 따른 실제 힘 배율
        public float maxForce = 20.0f;       // 최대 발사 제한 세기

        [Header("Trajectory Settings (궤적 예측)")]
        public LineRenderer trajectoryLine;    // 궤적을 그릴 LineRenderer 레퍼런스
        public int trajectoryPointCount = 30; // 궤적을 구성할 점의 개수 (많을수록 부드러움)
        public float timeStepPerPoint = 0.1f; // 각 점 사이의 시간 간격 (초)

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

            // LineRenderer 설정 초기화
            if (trajectoryLine == null) trajectoryLine = GetComponent<LineRenderer>();
            trajectoryLine.positionCount = trajectoryPointCount;
            trajectoryLine.enabled = false; // 평소에는 끔

            SpawnBlock();
        }

        private void Update()
        {
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
            // 3. 드래그 중일 때 (시각적 피드백 & 궤적 그리기)
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

            // ⭐ [추가] X축이 마이너스 방향(뒤쪽)이라면 0으로 고정하여 뒤로 발사되지 않게 막음
            if (dragVector.x < 5)
            {
                dragVector.x = 5;
            }

            Vector2 predictedForce = dragVector * forceMultiplier;
            if (predictedForce.magnitude > maxForce)
            {
                predictedForce = predictedForce.normalized * maxForce;
            }

            float blockMass = (currentBlock != null) ? currentBlock.GetComponent<Rigidbody2D>().mass : 1.0f;
            Vector2 initialVelocity = predictedForce / blockMass;

            DrawTrajectory(currentBlock.transform.position, initialVelocity);
        }

        private void FireBlock()
        {
            isDragging = false;
            trajectoryLine.enabled = false;

            if (currentBlock == null) return;

            Vector2 currentMousePos = GetMouseWorldPosition();
            Vector2 dragVector = dragStartPos - currentMousePos;

            // ⭐ [추가] 마우스를 오른쪽으로 당겨서 X축 발사 방향이 마이너스가 된 경우 방어
            if (dragVector.x < 5)
            {
                dragVector.x = 5;
            }

            // 최종 날아갈 방향과 크기 계산
            Vector2 shootForce = dragVector * forceMultiplier;

            if (shootForce.magnitude > maxForce)
            {
                shootForce = shootForce.normalized * maxForce;
            }

            // 블록 발사!
            currentBlock.Launch(shootForce);

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