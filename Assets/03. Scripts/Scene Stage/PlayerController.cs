using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Babel
{
    public class PlayerController : MonoBehaviour
    {
        #region Settings

        [Header("Settings")]
        public float speed = 8.0f;
        public float minXPos = -1.75f;
        public float maxXPos = 1.75f;

        [Header("Ref")]
        public Transform spawnPoint;
        private Block currentBlock;

        #endregion

        #region Unity Methods

        private void Start()
        {
            SpawnBlock();
        }

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
                DropBlock();
            }
            else if (Mouse.current.leftButton.isPressed)
            {
                UpdateMovement();
            }
        }

        #endregion

        #region Custom Methods

        private void UpdateMovement()
        {
            // 한 프레임 당 마우스 입력 변화량을 가지고 옴
            Vector2 mouseDelta = Mouse.current.delta.value;
            // 게임 내의 좌표계로 바꿔주면서 적용함
            Vector3 moveVector = new Vector3(mouseDelta.x, 0.0f, 0.0f) * speed * Time.deltaTime;

            Vector3 adjustPos = transform.position + moveVector;
            adjustPos.x = Mathf.Clamp(adjustPos.x, minXPos, maxXPos);

            transform.position = adjustPos;
        }

        private void SpawnBlock()
        {
            // MergeGameManager에서 공을 가지고 와서 SpawnPoint에 생성을 함

            Block spawnBlock = GameManager.Instance.GetNextBlock();
            // MergeGameManager에서 가져온 프리팹을 이용해 새로운 볼을 만들어준다.
            // Instantiate(견본, Transform 매개변수) : 매개변수로 전달된 Transform의 자식으로 복사본을 생성한다.
            currentBlock = Instantiate(spawnBlock, spawnPoint);
            // 생성한 볼을 spawnPoint의 위치로 초기화 해준다.
            currentBlock.transform.localPosition = Vector3.zero; // 부모 좌표와 동일하게 만들어줌
        }

        private void DropBlock()
        {
            if (currentBlock == null) return;
            currentBlock.Drop();
            StartCoroutine(SpawnDelayCoroutine(currentBlock));
            currentBlock = null;
        }


        private IEnumerator SpawnDelayCoroutine(Block dropBlock)
        {
            while (true)
            {
                if (dropBlock.HasFirstContact) break;

                yield return null;
            }

            SpawnBlock();
        }

        #endregion

    }
}