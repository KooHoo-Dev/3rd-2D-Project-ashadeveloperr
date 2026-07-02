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

        protected override void Awake()
        {

            Time.timeScale = 1.0f;
            onPause = false;
            Cam = Camera.main;
            resultScreenRectTransform = resultScreen.GetComponent<RectTransform>();
            inGameUIRectTransform = inGameUI.GetComponent<RectTransform>();
            InteractUIRightRectTransform = InteractUIRight.GetComponent<RectTransform>();
        }

        private void Start()
        {
            RectTransform canvasRect = GetComponent<RectTransform>();
            float screenHeight = canvasRect.rect.height;
            float screenWidth = canvasRect.rect.width;

            inGameUIRectTransform.anchoredPosition = new Vector2(0, screenHeight);
            InteractUIRightRectTransform.anchoredPosition = new Vector2(screenWidth, 0);
            SetCameraTarget(ship.transform, shipPosOffset, currentCamZoom);
        }
        private void Update()
        {
            SetScreensPosition();
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

        public void ShowResultWindow()
        {
            if (GameManager.Instance.isGameOver)
            {
                NextStageText.SetText("Retry");
            }
            else
            {
                NextStageText.SetText("Next");
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
                    resultScreenRectTransform.anchoredPosition = Vector2.Lerp(resultScreenRectTransform.anchoredPosition, resultScreenRootPosition, Time.deltaTime * 1.25f);
                }
                if (Vector2.Distance(inGameUIRectTransform.anchoredPosition, inGameUIRootPosition) >= 0.1f)
                {
                    inGameUIRectTransform.anchoredPosition = Vector2.Lerp(inGameUIRectTransform.anchoredPosition, inGameUIRootPosition, Time.deltaTime * 1.2f);
                }
                if (Vector2.Distance(InteractUIRightRectTransform.anchoredPosition, interactUIRightRootPosition) >= 0.1f)
                {
                    InteractUIRightRectTransform.anchoredPosition = Vector2.Lerp(InteractUIRightRectTransform.anchoredPosition, interactUIRightRootPosition, Time.deltaTime * 1.2f);
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
