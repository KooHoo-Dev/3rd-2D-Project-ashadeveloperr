using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Babel
{
    public class StandingCharacterView : MonoBehaviour
    {
        [SerializeField] private DialogueSlot slot;
        [SerializeField] private Image characterImage;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Vector2 hiddenOffset = new Vector2(500.0f, 0.0f);
        [SerializeField] private float moveDuration = 0.25f;
        [SerializeField] private Color focusedColor = Color.white;
        [SerializeField] private Color unfocusedColor = new Color(0.45f, 0.45f, 0.45f, 1.0f);

        private Vector2 visiblePosition;
        private Vector2 hiddenPosition;
        private Coroutine moveCoroutine;

        public DialogueSlot Slot => slot;
        public string CharacterId { get; private set; }
        public bool IsOccupied => string.IsNullOrWhiteSpace(CharacterId) == false;

        private void Awake()
        {
            if (characterImage == null)
            {
                characterImage = GetComponentInChildren<Image>(true);
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            visiblePosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
            hiddenPosition = visiblePosition + GetDirectionalHiddenOffset();
            ResetView();
        }

        public IEnumerator Enter(CharacterDefinition character, Sprite sprite, string characterId)
        {
            CharacterId = character != null ? character.CharacterId : characterId;

            if (characterImage != null)
            {
                characterImage.sprite = sprite != null ? sprite : character?.DefaultSprite;
                characterImage.color = focusedColor;
            }

            gameObject.SetActive(true);
            yield return MoveTo(visiblePosition, 1.0f);
        }

        public IEnumerator Exit()
        {
            yield return MoveTo(hiddenPosition, 0.0f);
            ResetView();
        }

        public void SetExpression(Sprite sprite)
        {
            if (characterImage != null && sprite != null)
            {
                characterImage.sprite = sprite;
            }
        }

        public void SetFocused(bool focused)
        {
            if (characterImage != null)
            {
                characterImage.color = focused ? focusedColor : unfocusedColor;
            }
        }

        public void ResetView()
        {
            CharacterId = string.Empty;

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = hiddenPosition;
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0.0f;
            }

            gameObject.SetActive(false);
        }

        private IEnumerator MoveTo(Vector2 targetPosition, float targetAlpha)
        {
            if (moveCoroutine != null)
            {
                StopCoroutine(moveCoroutine);
            }

            moveCoroutine = StartCoroutine(MoveRoutine(targetPosition, targetAlpha));
            yield return moveCoroutine;
        }

        private IEnumerator MoveRoutine(Vector2 targetPosition, float targetAlpha)
        {
            if (rectTransform == null || canvasGroup == null)
            {
                yield break;
            }

            Vector2 startPosition = rectTransform.anchoredPosition;
            float startAlpha = canvasGroup.alpha;
            float timer = 0.0f;

            while (timer < moveDuration)
            {
                timer += Time.unscaledDeltaTime;
                float t = moveDuration <= 0.0f ? 1.0f : Mathf.Clamp01(timer / moveDuration);
                rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            rectTransform.anchoredPosition = targetPosition;
            canvasGroup.alpha = targetAlpha;
            moveCoroutine = null;
        }

        private Vector2 GetDirectionalHiddenOffset()
        {
            return slot == DialogueSlot.L1 || slot == DialogueSlot.L2 || slot == DialogueSlot.L3 ? -hiddenOffset : hiddenOffset;
        }
    }
}
