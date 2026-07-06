using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace Babel
{

    public class UIManager : SingletonBase<UIManager>
    {
        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        public GameObject resultScreen;
        public GameObject inGameUI;
        public GameObject pauseUI;
        public GameObject InteractUIRight;
        public GameObject pauseUIScreen;
        public TMP_Text NextStageText;
        public TMP_Text ResultTitle;

        private Camera Cam { get; set; }
        private RectTransform resultScreenRectTransform;
        private RectTransform inGameUIRectTransform;
        private RectTransform InteractUIRightRectTransform;

        private Vector2 resultScreenRootPosition;
        private Vector2 inGameUIRootPosition;
        private Vector2 interactUIRightRootPosition;
        private bool onResult = false;
        public bool onPause = false;

        public Ship ship;

        private Transform currentTarget;
        private Vector3 currentOffset;
        public Vector3 shipPosOffset = new Vector3(-10, 7, 0);
        public Vector3 throwerOffset = new Vector3(-15, 7, 0);
        public Vector3 blockOffset = new Vector3(0, 0, 0);
        private float currentCamZoom = 20;
        private float currentCamZoomTarget = 20;

        [Header("튜토리얼 스플래시 UI")]
        [SerializeField] private GameObject tutorialSplashUI;
        [SerializeField] private RectTransform tutorialSplashRectTransform;
        [SerializeField] private float tutorialSplashHideDuration = 0.6f;
        private Vector2 tutorialSplashStartHidePosition;
        private float tutorialSplashHideTimer;

        public bool IsTutorialSplashActive { get; private set; }

        private Vector2 tutorialSplashRootPosition;
        private Vector2 tutorialSplashHidePosition;
        private bool isTutorialSplashHiding;

        [Header("바람 UI")]
        public TMP_Text windText;

        protected override void Awake()
        {
            Time.timeScale = 1.0f;
            onPause = false;

            Cam = Camera.main;

            resultScreenRectTransform = resultScreen.GetComponent<RectTransform>();
            inGameUIRectTransform = inGameUI.GetComponent<RectTransform>();
            InteractUIRightRectTransform = InteractUIRight.GetComponent<RectTransform>();

            if (tutorialSplashUI != null && tutorialSplashRectTransform == null)
            {
                tutorialSplashRectTransform = tutorialSplashUI.GetComponent<RectTransform>();
            }
        }

        private void Start()
        {
            if (ship == null && GameManager.Instance != null)
            {
                ship = GameManager.Instance.ship;
            }

            RectTransform canvasRect = GetComponent<RectTransform>();
            float screenHeight = canvasRect.rect.height;
            float screenWidth = canvasRect.rect.width;

            inGameUIRectTransform.anchoredPosition = new Vector2(0, screenHeight);
            InteractUIRightRectTransform.anchoredPosition = new Vector2(screenWidth, 0);

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

            if (GameManager.Instance.isGameOver)
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

            tutorialSplashHidePosition =
                tutorialSplashRootPosition +
                new Vector2(0, screenHeight + tutorialSplashRectTransform.rect.height);

            bool shouldShow =
                TotalGameManager.Instance != null &&
                TotalGameManager.Instance.HasSeenStageTutorialSplash == false;

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

                tutorialSplashRectTransform.anchoredPosition = Vector2.Lerp(
                    tutorialSplashStartHidePosition,
                    tutorialSplashHidePosition,
                    t
                );

                if (t >= 1.0f)
                {
                    EndTutorialSplash();
                }
            }

            return true;
        }

        private bool WasTutorialDismissInputPressed()
        {
            bool keyboardPressed =
                Keyboard.current != null &&
                Keyboard.current.anyKey.wasPressedThisFrame;

            bool mousePressed =
                Mouse.current != null &&
                (
                    Mouse.current.leftButton.wasPressedThisFrame ||
                    Mouse.current.rightButton.wasPressedThisFrame ||
                    Mouse.current.middleButton.wasPressedThisFrame
                );

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

            Vector3 lerpPos = Vector3.Lerp(Cam.transform.position, (currentTarget.position + currentOffset), 1.8f * Time.deltaTime);
            lerpPos.z = Cam.transform.position.z;
            Cam.orthographicSize = Mathf.Lerp(currentCamZoom, currentCamZoomTarget, 2.5f * Time.deltaTime);
            currentCamZoom = Cam.orthographicSize;
            Cam.transform.position = lerpPos;
        }

        private void CameraReset()
        {
            Vector3 offset = new Vector3(0, 18, 0);
            Vector3 lerpPos = Vector3.Lerp(Cam.transform.position, Vector3.zero + offset, 0.4f * Time.deltaTime);
            lerpPos.z = Cam.transform.position.z;
            Cam.orthographicSize = Mathf.Lerp(currentCamZoom, currentCamZoomTarget, 2.5f * Time.deltaTime);
            Cam.transform.position = lerpPos;
        }

        public void ShowResultWindow(int totalClosing)
        {
            if (GameManager.Instance.isFailed)
            {
                NextStageText.SetText("Restart");
                ResultTitle.SetText("SUNK...");
            }
            else
            {
                if (totalClosing <= 0)
                {
                    NextStageText.SetText("Retry");
                    ResultTitle.SetText("ALIVE?");
                }
                else
                {
                    NextStageText.SetText("Next");
                    ResultTitle.SetText("SAIL!");
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
                if (Vector2.Distance(InteractUIRightRectTransform.anchoredPosition, interactUIRightRootPosition) >= 0.1f)
                {
                    InteractUIRightRectTransform.anchoredPosition = Vector2.Lerp(InteractUIRightRectTransform.anchoredPosition, interactUIRightRootPosition, Time.deltaTime * 1.5f);
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

            if (GameManager.Instance.sinkPoint > 0)
            {
                interactUIRightRootPosition = Vector2.zero;
            }
            else
            {
                RectTransform canvasRect = GetComponent<RectTransform>();
                float screenWidth = canvasRect.rect.width;
                interactUIRightRootPosition = new Vector2(screenWidth, 0);
            }


            if (Vector2.Distance(InteractUIRightRectTransform.anchoredPosition, interactUIRightRootPosition) >= 0.1f)
            {
                InteractUIRightRectTransform.anchoredPosition = Vector2.Lerp(InteractUIRightRectTransform.anchoredPosition, interactUIRightRootPosition, Time.deltaTime * 2.5f);
            }
        }
    }
}
