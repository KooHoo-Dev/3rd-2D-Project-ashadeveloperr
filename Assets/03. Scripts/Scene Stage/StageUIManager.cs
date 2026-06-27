using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Babel
{

    public class StageUIManager : MonoBehaviour
    {
        private Camera Cam { get; set; }
        private RectTransform rectTransform;
        private Canvas parentCanvas;

        private float lerpSpeed = 2.0f;
        private Vector2 targetPosition;
        private bool isMoving = false;

        public Ship ship;

        private void Awake()
        {
            Cam = Camera.main;
            rectTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();
        }

        private void Start()
        {
            
        }
        private void Update()
        {
            MoveResultWindow();
            if (GameManager.Instance.isGameOver)
            {
                CameraReset();
            }
            else
            {
                CameraWork();
            }
        }


        private void CameraWork()
        {
            Vector3 offset = new Vector3(0, 7, 0);
            Vector3 lerpPos = Vector3.Lerp(Cam.transform.position, ship.transform.position + offset, 0.7f * Time.deltaTime);
            lerpPos.z = Cam.transform.position.z;  // 2D니까 z값은 바뀔 필요 없음
            Cam.transform.position = lerpPos;
        }

        private void CameraReset()
        {
            Vector3 offset = new Vector3(0, 18, 0);
            Vector3 lerpPos = Vector3.Lerp(Cam.transform.position, Vector3.zero + offset, 0.4f * Time.deltaTime);
            lerpPos.z = Cam.transform.position.z;  // 2D니까 z값은 바뀔 필요 없음
            Cam.transform.position = lerpPos;
        }

        public void ShowResultWindow()
        {
            targetPosition = Vector2.zero;
            isMoving = true;
        }
        
        private void MoveResultWindow()
        {
            if (isMoving)
            {
                if (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) >= 0.1f)
                {
                    rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, Time.deltaTime * lerpSpeed);
                }
            }
            else
            {
                RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
                float screenHeight = canvasRect.rect.height;

                rectTransform.anchoredPosition = new Vector2(0, -screenHeight);

                targetPosition = rectTransform.anchoredPosition;
            }
        }
    }
}
