using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Babel
{
    public class UIManager : SingletonBase<UIManager>
    {
        [Header("화면 UI")]
        [SerializeField] private GameObject resultScreen;
        [SerializeField] private GameObject inGameUI;
        [SerializeField] private GameObject pauseUI;
        [FormerlySerializedAs("InteractUIRight")]
        [SerializeField] private GameObject interactUIRight;
        [SerializeField] private GameObject pauseUIScreen;

        [Header("결과창 텍스트")]
        [FormerlySerializedAs("NextStageText")]
        [SerializeField] private TMP_Text nextStageText;
        [FormerlySerializedAs("ResultTitle")]
        [SerializeField] private TMP_Text resultTitle;

        [Header("바람 UI")]
        [SerializeField] private TMP_Text windText;

        [Header("카메라 오프셋")]
        [FormerlySerializedAs("shipPosOffset")]
        [SerializeField] private Vector3 shipPosOffset = new Vector3(-10, 7, 0);
        [FormerlySerializedAs("throwerOffset")]
        [SerializeField] private Vector3 throwerOffset = new Vector3(-15, 7, 0);
        [FormerlySerializedAs("blockOffset")]
        [SerializeField] private Vector3 blockOffset = Vector3.zero;

        [Header("튜토리얼 스플래시 UI")]
        [SerializeField] private GameObject tutorialSplashUI;
        [SerializeField] private RectTransform tutorialSplashRectTransform;
        [SerializeField] private float tutorialSplashHideDuration = 0.6f;

        private Camera cam;
        private RectTransform resultScreenRectTransform;
        private RectTransform inGameUIRectTransform;
        private RectTransform interactUIRightRectTransform;
        private Ship ship;
        private Transform currentTarget;
        private Vector2 resultScreenRootPosition;
        private Vector2 inGameUIRootPosition;
        private Vector2 interactUIRightRootPosition;
        private Vector2 tutorialSplashRootPosition;
        private Vector2 tutorialSplashHidePosition;
        private Vector2 tutorialSplashStartHidePosition;
        private Vector3 currentOffset;
        private float currentCamZoom = 20f;
        private float currentCamZoomTarget = 20f;
        private bool onResult = false;
        private bool onPause = false;
        private bool isTutorialSplashHiding;
        private float tutorialSplashHideTimer;

        public bool IsTutorialSplashActive { get; private set; }
        public Ship CurrentShip => ship;
        public Vector3 ShipPosOffset => shipPosOffset;
        public Vector3 ThrowerOffset => throwerOffset;
        public Vector3 BlockOffset => blockOffset;

        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        protected override void Awake()
        {
            base.Awake();

            if (instance != this)
            {
                return;
            }

            Time.timeScale = 1.0f;
            onPause = false;
            cam = Camera.main;
            resultScreenRectTransform = resultScreen.GetComponent<RectTransform>();
            inGameUIRectTransform = inGameUI.GetComponent<RectTransform>();
            interactUIRightRectTransform = interactUIRight.GetComponent<RectTransform>();

            if (tutorialSplashUI != null && tutorialSplashRectTransform == null)
            {
                tutorialSplashRectTransform = tutorialSplashUI.GetComponent<RectTransform>();
            }
        }

        private void Start()
        {
            if (ship == null && GameManager.Instance != null)
            {
                ship = GameManager.Instance.CurrentShip;
            }

            RectTransform canvasRect = GetComponent<RectTransform>();
            float screenHeight = canvasRect.rect.height;
            float screenWidth = canvasRect.rect.width;
            inGameUIRectTransform.anchoredPosition = new Vector2(0, screenHeight);
            interactUIRightRectTransform.anchoredPosition = new Vector2(screenWidth, 0);

            if (ship != null)
            {
                SetCameraTarget(ship.transform, shipPosOffset, currentCamZoom);
            }

            InitializeTutorialSplash(screenHeight);
        }

        private void Update()
        {
            SetScreensPosition();

            if (UpdateTutorialSplash())
            {
                return;
            }

            if (GameManager.Instance.IsGameOver)
            {
                CameraReset();
            }
            else
            {
                SetInGameUI();
                CameraWork();

                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    TogglePause();
                }
            }

            SinkingUI();
        }

        public void SetShip(Ship newShip)
        {
            ship = newShip;
        }

        private void EndTutorialSplash()
        {
            if (tutorialSplashRectTransform != null)
            {
                tutorialSplashRectTransform.anchoredPosition = tutorialSplashHidePosition;
            }

            if (tutorialSplashUI != null)
            {
                tutorialSplashUI.SetActive(false);
            }

            IsTutorialSplashActive = false;
            isTutorialSplashHiding = false;
            tutorialSplashHideTimer = 0.0f;
            onPause = false;

            if (pauseUI != null)
            {
                pauseUI.SetActive(false);
            }

            if (pauseUIScreen != null)
            {
                pauseUIScreen.SetActive(false);
            }

            Time.timeScale = 1.0f;
        }

        private void InitializeTutorialSplash(float screenHeight)
        {
            if (tutorialSplashUI == null || tutorialSplashRectTransform == null)
            {
                IsTutorialSplashActive = false;
                isTutorialSplashHiding = false;
                tutorialSplashHideTimer = 0.0f;
                Time.timeScale = 1.0f;
                return;
            }

            tutorialSplashRootPosition = tutorialSplashRectTransform.anchoredPosition;
            tutorialSplashHidePosition = tutorialSplashRootPosition + new Vector2(0, screenHeight + tutorialSplashRectTransform.rect.height);
            bool shouldShow = TotalGameManager.Instance != null && TotalGameManager.Instance.HasSeenStageTutorialSplash == false;
            IsTutorialSplashActive = shouldShow;
            isTutorialSplashHiding = false;
            tutorialSplashHideTimer = 0.0f;

            if (shouldShow)
            {
                tutorialSplashUI.SetActive(true);
                tutorialSplashRectTransform.anchoredPosition = tutorialSplashRootPosition;
                Time.timeScale = 0.0f;
            }
            else
            {
                tutorialSplashRectTransform.anchoredPosition = tutorialSplashHidePosition;
                tutorialSplashUI.SetActive(false);
                Time.timeScale = 1.0f;
            }
        }

        private bool UpdateTutorialSplash()
        {
            if (IsTutorialSplashActive == false)
            {
                return false;
            }

            if (tutorialSplashUI == null || tutorialSplashRectTransform == null)
            {
                EndTutorialSplash();
                return true;
            }

            if (isTutorialSplashHiding == false && WasTutorialDismissInputPressed())
            {
                isTutorialSplashHiding = true;
                tutorialSplashHideTimer = 0.0f;
                tutorialSplashStartHidePosition = tutorialSplashRectTransform.anchoredPosition;

                if (TotalGameManager.Instance != null)
                {
                    TotalGameManager.Instance.MarkStageTutorialSplashSeen();
                }
            }

            if (isTutorialSplashHiding)
            {
                tutorialSplashHideTimer += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(tutorialSplashHideTimer / tutorialSplashHideDuration);
                tutorialSplashRectTransform.anchoredPosition = Vector2.Lerp(tutorialSplashStartHidePosition, tutorialSplashHidePosition, t);

                if (t >= 1.0f)
                {
                    EndTutorialSplash();
                }
            }

            return true;
        }

        private bool WasTutorialDismissInputPressed()
        {
            bool keyboardPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
            bool mousePressed = Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame || Mouse.current.middleButton.wasPressedThisFrame);
            return keyboardPressed || mousePressed;
        }

        public void UpdateWindDisplay(float windSpeed)
        {
            if (windText == null) return;

            float absSpeed = Mathf.Abs(windSpeed);

            if (absSpeed < 0.05f)
            {
                windText.SetText("-- 0.0m/s");
                return;
            }

            string directionText = windSpeed < 0 ? "<<" : ">>";
            windText.SetText($"{directionText} {absSpeed:0.0}m/s");
        }

        public void TogglePause()
        {
            if (onPause) Resume();
            else Pause();
        }

        private void Pause()
        {
            Time.timeScale = 0.0f;
            pauseUI.SetActive(true);
            pauseUIScreen.SetActive(true);
            onPause = true;
        }

        private void Resume()
        {
            Time.timeScale = 1.0f;
            pauseUI.SetActive(false);
            pauseUIScreen.SetActive(false);
            onPause = false;
        }

        public void SetCameraTarget(Transform newTarget, Vector3 offset, float size)
        {
            if (newTarget != null)
            {
                currentTarget = newTarget;
                currentOffset = offset;
                currentCamZoomTarget = size;
            }
        }

        private void CameraWork()
        {
            if (currentTarget == null)
            {
                SetCameraTarget(ship.transform, shipPosOffset, currentCamZoom);
            }

            Vector3 lerpPos = Vector3.Lerp(cam.transform.position, currentTarget.position + currentOffset, 1.8f * Time.deltaTime);
            lerpPos.z = cam.transform.position.z;
            cam.orthographicSize = Mathf.Lerp(currentCamZoom, currentCamZoomTarget, 2.5f * Time.deltaTime);
            currentCamZoom = cam.orthographicSize;
            cam.transform.position = lerpPos;
        }

        private void CameraReset()
        {
            Vector3 offset = new Vector3(0, 18, 0);
            Vector3 lerpPos = Vector3.Lerp(cam.transform.position, Vector3.zero + offset, 0.4f * Time.deltaTime);
            lerpPos.z = cam.transform.position.z;
            cam.orthographicSize = Mathf.Lerp(currentCamZoom, currentCamZoomTarget, 2.5f * Time.deltaTime);
            cam.transform.position = lerpPos;
        }

        public void ShowResultWindow(int totalClosing)
        {
            if (GameManager.Instance.IsFailed)
            {
                nextStageText.SetText("Restart");
                resultTitle.SetText("SUNK...");
            }
            else
            {
                if (totalClosing <= 0)
                {
                    nextStageText.SetText("Retry");
                    resultTitle.SetText("ALIVE?");
                }
                else
                {
                    nextStageText.SetText("Next");
                    resultTitle.SetText("SAIL!");
                }
            }

            RectTransform canvasRect = GetComponent<RectTransform>();
            float screenHeight = canvasRect.rect.height;
            float screenWidth = canvasRect.rect.width;
            resultScreenRootPosition = Vector2.zero;
            inGameUIRootPosition = new Vector2(0, screenHeight);
            interactUIRightRootPosition = new Vector2(0, screenWidth);
            onResult = true;
        }

        private void SetScreensPosition()
        {
            if (onResult)
            {
                if (Vector2.Distance(resultScreenRectTransform.anchoredPosition, resultScreenRootPosition) >= 0.1f)
                {
                    resultScreenRectTransform.anchoredPosition = Vector2.Lerp(resultScreenRectTransform.anchoredPosition, resultScreenRootPosition, Time.deltaTime * 3.5f);
                }

                if (Vector2.Distance(inGameUIRectTransform.anchoredPosition, inGameUIRootPosition) >= 0.1f)
                {
                    inGameUIRectTransform.anchoredPosition = Vector2.Lerp(inGameUIRectTransform.anchoredPosition, inGameUIRootPosition, Time.deltaTime * 1.5f);
                }

                if (Vector2.Distance(interactUIRightRectTransform.anchoredPosition, interactUIRightRootPosition) >= 0.1f)
                {
                    interactUIRightRectTransform.anchoredPosition = Vector2.Lerp(interactUIRightRectTransform.anchoredPosition, interactUIRightRootPosition, Time.deltaTime * 1.5f);
                }
            }
            else
            {
                RectTransform canvasRect = GetComponent<RectTransform>();
                float screenHeight = canvasRect.rect.height;
                float screenWidth = canvasRect.rect.width;
                interactUIRightRootPosition = new Vector2(screenWidth, 0);
                resultScreenRectTransform.anchoredPosition = new Vector2(0, -screenHeight);
                resultScreenRootPosition = resultScreenRectTransform.anchoredPosition;
            }
        }

        private void SetInGameUI()
        {
            if (Vector2.Distance(inGameUIRectTransform.anchoredPosition, resultScreenRootPosition) >= 0.1f)
            {
                inGameUIRectTransform.anchoredPosition = Vector2.Lerp(inGameUIRectTransform.anchoredPosition, inGameUIRootPosition, Time.deltaTime * 1.7f);
            }
        }

        private void SinkingUI()
        {
            if (onResult) return;

            if (GameManager.Instance.SinkPoint > 0)
            {
                interactUIRightRootPosition = Vector2.zero;
            }
            else
            {
                RectTransform canvasRect = GetComponent<RectTransform>();
                float screenWidth = canvasRect.rect.width;
                interactUIRightRootPosition = new Vector2(screenWidth, 0);
            }

            if (Vector2.Distance(interactUIRightRectTransform.anchoredPosition, interactUIRightRootPosition) >= 0.1f)
            {
                interactUIRightRectTransform.anchoredPosition = Vector2.Lerp(interactUIRightRectTransform.anchoredPosition, interactUIRightRootPosition, Time.deltaTime * 2.5f);
            }
        }
    }
}
