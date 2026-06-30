using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        private Camera Cam { get; set; }
        private RectTransform resultScreenRectTransform;
        private RectTransform inGameUIRectTransform;

        private float lerpSpeed = 1.4f;
        private Vector2 resultScreenRootPosition;
        private Vector2 inGameUIRootPosition;
        private bool onResult = false;

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
            Cam = Camera.main;
            resultScreenRectTransform = resultScreen.GetComponent<RectTransform>();
            inGameUIRectTransform = inGameUI.GetComponent<RectTransform>();
        }

        private void Start()
        {
            RectTransform canvasRect = GetComponent<RectTransform>();
            float screenHeight = canvasRect.rect.height;

            inGameUIRectTransform.anchoredPosition = new Vector2(0, screenHeight);
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
            }
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
            RectTransform canvasRect = GetComponent<RectTransform>();
            float screenHeight = canvasRect.rect.height;

            resultScreenRootPosition = Vector2.zero;
            inGameUIRootPosition = new Vector2(0, screenHeight);
            onResult = true;
        }
        
        private void SetScreensPosition()
        {
            if (onResult)
            {
                if (Vector2.Distance(resultScreenRectTransform.anchoredPosition, resultScreenRootPosition) >= 0.1f)
                {
                    resultScreenRectTransform.anchoredPosition = Vector2.Lerp(resultScreenRectTransform.anchoredPosition, resultScreenRootPosition, Time.deltaTime * lerpSpeed);
                }
                if (Vector2.Distance(inGameUIRectTransform.anchoredPosition, inGameUIRootPosition) >= 0.1f)
                {
                    inGameUIRectTransform.anchoredPosition = Vector2.Lerp(inGameUIRectTransform.anchoredPosition, inGameUIRootPosition, Time.deltaTime * lerpSpeed);
                }
            }
            else
            {
                RectTransform canvasRect = GetComponent<RectTransform>();
                float screenHeight = canvasRect.rect.height;

                resultScreenRectTransform.anchoredPosition = new Vector2(0, -screenHeight);

                resultScreenRootPosition = resultScreenRectTransform.anchoredPosition;
            }
        }

        private void SetInGameUI()
        {
            if (Vector2.Distance(inGameUIRectTransform.anchoredPosition, resultScreenRootPosition) >= 0.1f)
            {
                inGameUIRectTransform.anchoredPosition = Vector2.Lerp(inGameUIRectTransform.anchoredPosition, inGameUIRootPosition, Time.deltaTime * lerpSpeed);
            }
        }
    }
}
