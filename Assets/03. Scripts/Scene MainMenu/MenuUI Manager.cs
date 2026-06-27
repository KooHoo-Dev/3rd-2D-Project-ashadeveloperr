using UnityEngine;
using UnityEngine.SceneManagement;

namespace Babel
{

    public class MainMenuUIManager : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Canvas parentCanvas;

        private float lerpSpeed = 2.0f;
        private Vector2 targetPosition;
        private bool isMoving = false;

        public Transform cameraTargetPivot;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();
        }

        private void Start()
        {
            MoveToTarget();
        }
        private void Update()
        {
            CameraMoving();
        }


        public void MoveToTarget()
        {
            targetPosition = Vector2.zero;
            isMoving = true;
        }

        private void CameraMoving()
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
