using UnityEngine;

namespace Babel
{
    public class MainMenuUIManager : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private RectTransform titleText;
        [SerializeField] private RectTransform root;

        [Header("카메라 참조")]
        [SerializeField] private Transform cameraTargetPivot;

        [Header("UI 이동 설정")]
        [SerializeField] private float titleMoveSpeed = 2.0f;
        [SerializeField] private float rootMoveSpeed = 2.0f;
        [SerializeField] private float titleStartDelay = 0.1f;
        [SerializeField] private float rootStartDelay = 0.25f;

        [Header("카메라 이동 설정")]
        [SerializeField] private Vector3 cameraStartOffset = new Vector3(0, 8, 0);
        [SerializeField] private Vector3 cameraTargetOffset = Vector3.zero;
        [SerializeField] private float cameraMoveSpeed = 1.2f;

        [Header("카메라 부유 연출")]
        [SerializeField] private float floatingAmount = 0.25f;
        [SerializeField] private float floatingSpeed = 0.6f;
        [SerializeField] private float floatingFollowSpeed = 1.2f;

        private Camera cam;
        private RectTransform canvasRectTransform;
        private Vector2 titleRootPosition;
        private Vector2 rootRootPosition;
        private float introTimer;
        private float floatingTimer;
        private float cameraZ;
        private bool cameraIntroFinished = false;

        private void Awake()
        {
            Time.timeScale = 1.0f;
            cam = Camera.main;
            canvasRectTransform = GetComponent<RectTransform>();
            cameraZ = cam.transform.position.z;

            if (titleText == null)
            {
                Transform titleTransform = transform.Find("TitleText");
                if (titleTransform != null)
                {
                    titleText = titleTransform.GetComponent<RectTransform>();
                }
            }

            if (root == null)
            {
                Transform rootTransform = transform.Find("Root");
                if (rootTransform != null)
                {
                    root = rootTransform.GetComponent<RectTransform>();
                }
            }
        }

        private void Start()
        {
            SetStartPositions();
        }

        private void Update()
        {
            introTimer += Time.deltaTime;
            MoveTitleText();
            MoveRoot();
            CameraWork();
        }

        private void SetStartPositions()
        {
            float screenHeight = canvasRectTransform.rect.height;
            float screenWidth = canvasRectTransform.rect.width;

            if (titleText != null)
            {
                titleRootPosition = titleText.anchoredPosition;
                titleText.anchoredPosition = titleRootPosition + new Vector2(0, screenHeight + titleText.rect.height);
            }

            if (root != null)
            {
                rootRootPosition = root.anchoredPosition;
                root.anchoredPosition = rootRootPosition + new Vector2(screenWidth + root.rect.width, 0);
            }

            if (cam != null)
            {
                Vector3 targetPos = GetCameraBasePosition();
                cam.transform.position = targetPos + cameraStartOffset;
            }
        }

        private void MoveTitleText()
        {
            if (titleText == null) return;
            if (introTimer < titleStartDelay) return;

            if (Vector2.Distance(titleText.anchoredPosition, titleRootPosition) >= 0.1f)
            {
                titleText.anchoredPosition = Vector2.Lerp(titleText.anchoredPosition, titleRootPosition, Time.deltaTime * titleMoveSpeed);
            }
        }

        private void MoveRoot()
        {
            if (root == null) return;
            if (introTimer < rootStartDelay) return;

            if (Vector2.Distance(root.anchoredPosition, rootRootPosition) >= 0.1f)
            {
                root.anchoredPosition = Vector2.Lerp(root.anchoredPosition, rootRootPosition, Time.deltaTime * rootMoveSpeed);
            }
        }

        private void CameraWork()
        {
            if (cam == null) return;

            Vector3 baseTargetPos = GetCameraBasePosition();
            Vector3 targetPos = baseTargetPos;

            if (cameraIntroFinished == false)
            {
                if (Vector2.Distance(cam.transform.position, baseTargetPos) <= 0.1f)
                {
                    cameraIntroFinished = true;
                }
            }
            else
            {
                floatingTimer += Time.deltaTime;
                float floatingY = Mathf.Sin(floatingTimer * floatingSpeed) * floatingAmount;
                targetPos.y += floatingY;
            }

            float speed = cameraIntroFinished ? floatingFollowSpeed : cameraMoveSpeed;
            Vector3 lerpPos = Vector3.Lerp(cam.transform.position, targetPos, Time.deltaTime * speed);
            lerpPos.z = cameraZ;
            cam.transform.position = lerpPos;
        }

        private Vector3 GetCameraBasePosition()
        {
            Vector3 targetPos = cameraTargetPivot != null ? cameraTargetPivot.position + cameraTargetOffset : cam.transform.position;
            targetPos.z = cameraZ;
            return targetPos;
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
