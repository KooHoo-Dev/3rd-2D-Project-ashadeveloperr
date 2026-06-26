using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Babel
{

    public class StageUIManager : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Canvas parentCanvas;

        private float lerpSpeed = 1.5f;
        private Vector2 targetPosition;
        private bool isMoving = false;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            parentCanvas = GetComponentInParent<Canvas>();
        }

        private void Start()
        {
            InitPosition();
        }
        private void Update()
        {
            MoveResultWindow();
        }


        private void InitPosition()
        {
            RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();
            float screenHeight = canvasRect.rect.height;

            rectTransform.anchoredPosition = new Vector2(0, -screenHeight);

            targetPosition = rectTransform.anchoredPosition;
        }

        public void ShowResultWindow()
        {
            targetPosition = Vector2.zero;
            isMoving = true;
        }
        
        private void MoveResultWindow()
        {
            if (!isMoving) return;

            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, Time.deltaTime * lerpSpeed);

            if (Vector2.Distance(rectTransform.anchoredPosition, targetPosition) < 0.1f)
            {
                rectTransform.anchoredPosition = targetPosition;
                isMoving = false;
            }
        }
    }
}
