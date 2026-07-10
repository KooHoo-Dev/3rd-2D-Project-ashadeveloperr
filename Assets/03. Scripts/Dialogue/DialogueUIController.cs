using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Babel
{
    public class DialogueUIController : MonoBehaviour
    {
        [Header("대화창")]
        [SerializeField] private GameObject dialogueRoot;
        [SerializeField] private RectTransform dialoguePanel;
        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private float panelMoveDuration = 0.25f;
        [SerializeField] private float typeInterval = 0.03f;

        [Header("스킵 확인 팝업")]
        [SerializeField] private GameObject skipConfirmPopup;

        [Header("스탠딩 일러스트 슬롯")]
        [SerializeField] private StandingCharacterView[] standingCharacterViews;

        private Vector2 panelVisiblePosition;
        private Vector2 panelHiddenPosition;
        private bool isTyping;
        private bool lineAdvanceRequested;
        private bool skipConfirmRequested;
        private bool skipConfirmed;

        public bool SkipConfirmed => skipConfirmed;
        public bool HasRequiredReferences => dialogueRoot != null && dialoguePanel != null && dialogueText != null;

        private void Awake()
        {
            if (dialoguePanel != null)
            {
                panelVisiblePosition = dialoguePanel.anchoredPosition;
                RectTransform canvasRect = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
                float screenHeight = canvasRect != null ? canvasRect.rect.height : Screen.height;
                panelHiddenPosition = panelVisiblePosition - new Vector2(0.0f, screenHeight + dialoguePanel.rect.height);
                dialoguePanel.anchoredPosition = panelHiddenPosition;
            }

            if (dialogueRoot != null)
            {
                dialogueRoot.SetActive(false);
            }

            if (skipConfirmPopup != null)
            {
                skipConfirmPopup.SetActive(false);
            }
        }

        private void Update()
        {
            if (dialogueRoot == null || dialogueRoot.activeSelf == false)
            {
                return;
            }

            if (skipConfirmPopup != null && skipConfirmPopup.activeSelf)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                RequestSkipConfirm();
                return;
            }

            if (WasAdvanceInputPressed())
            {
                lineAdvanceRequested = true;
            }
        }

        public IEnumerator Show()
        {
            skipConfirmed = false;
            skipConfirmRequested = false;
            lineAdvanceRequested = false;

            if (dialogueRoot != null)
            {
                dialogueRoot.SetActive(true);
            }

            if (skipConfirmPopup != null)
            {
                skipConfirmPopup.SetActive(false);
            }

            yield return MovePanel(panelHiddenPosition, panelVisiblePosition);
        }

        public IEnumerator Hide()
        {
            yield return MovePanel(dialoguePanel != null ? dialoguePanel.anchoredPosition : panelVisiblePosition, panelHiddenPosition);

            if (dialogueRoot != null)
            {
                dialogueRoot.SetActive(false);
            }

            if (skipConfirmPopup != null)
            {
                skipConfirmPopup.SetActive(false);
            }

            ResetStandingCharacters();
        }

        public IEnumerator Speak(DialogueLine line, CharacterDefinition character, Sprite sprite)
        {
            FocusSpeaker(line.SpeakerId);
            SetSpeakerName(character != null ? character.DisplayName : line.SpeakerId);

            StandingCharacterView view = FindViewByCharacter(line.SpeakerId);

            if (view != null)
            {
                view.SetExpression(sprite);
            }

            yield return TypeText(line.Text);
        }

        public IEnumerator EnterCharacter(DialogueLine line, CharacterDefinition character, Sprite sprite)
        {
            StandingCharacterView view = FindViewBySlot(line.Slot);

            if (view == null)
            {
                yield break;
            }

            yield return view.Enter(character, sprite, line.SpeakerId);
            FocusSpeaker(line.SpeakerId);
        }

        public IEnumerator ExitCharacter(DialogueLine line)
        {
            StandingCharacterView view = FindViewByCharacter(line.SpeakerId);

            if (view == null)
            {
                view = FindViewBySlot(line.Slot);
            }

            if (view == null)
            {
                yield break;
            }

            yield return view.Exit();
        }

        public void FocusSpeaker(string speakerId)
        {
            if (standingCharacterViews == null)
            {
                return;
            }

            foreach (StandingCharacterView view in standingCharacterViews)
            {
                if (view == null || view.IsOccupied == false)
                {
                    continue;
                }

                view.SetFocused(string.Equals(view.CharacterId, speakerId, System.StringComparison.OrdinalIgnoreCase));
            }
        }

        public void ConfirmSkip()
        {
            skipConfirmed = true;
            skipConfirmRequested = false;

            if (skipConfirmPopup != null)
            {
                skipConfirmPopup.SetActive(false);
            }
        }

        public void CancelSkip()
        {
            skipConfirmRequested = false;

            if (skipConfirmPopup != null)
            {
                skipConfirmPopup.SetActive(false);
            }
        }

        public void RequestSkipConfirm()
        {
            skipConfirmRequested = true;

            if (skipConfirmPopup != null)
            {
                skipConfirmPopup.SetActive(true);
            }
            else
            {
                skipConfirmed = true;
            }
        }

        private IEnumerator TypeText(string text)
        {
            isTyping = true;
            lineAdvanceRequested = false;
            string safeText = text ?? string.Empty;

            if (dialogueText != null)
            {
                dialogueText.SetText(string.Empty);
            }

            for (int i = 0; i < safeText.Length; i++)
            {
                if (skipConfirmed)
                {
                    break;
                }

                if (lineAdvanceRequested)
                {
                    if (dialogueText != null)
                    {
                        dialogueText.SetText(safeText);
                    }

                    lineAdvanceRequested = false;
                    break;
                }

                if (dialogueText != null)
                {
                    dialogueText.SetText(safeText.Substring(0, i + 1));
                }

                float timer = 0.0f;

                while (timer < typeInterval)
                {
                    if (skipConfirmed || lineAdvanceRequested)
                    {
                        break;
                    }

                    timer += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            if (dialogueText != null && skipConfirmed == false)
            {
                dialogueText.SetText(safeText);
            }

            isTyping = false;
            lineAdvanceRequested = false;

            while (skipConfirmed == false && lineAdvanceRequested == false)
            {
                yield return null;
            }

            lineAdvanceRequested = false;
        }

        private IEnumerator MovePanel(Vector2 startPosition, Vector2 targetPosition)
        {
            if (dialoguePanel == null)
            {
                yield break;
            }

            float timer = 0.0f;

            while (timer < panelMoveDuration)
            {
                timer += Time.unscaledDeltaTime;
                float t = panelMoveDuration <= 0.0f ? 1.0f : Mathf.Clamp01(timer / panelMoveDuration);
                dialoguePanel.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            dialoguePanel.anchoredPosition = targetPosition;
        }

        private StandingCharacterView FindViewBySlot(DialogueSlot slot)
        {
            if (standingCharacterViews == null)
            {
                return null;
            }

            foreach (StandingCharacterView view in standingCharacterViews)
            {
                if (view != null && view.Slot == slot)
                {
                    return view;
                }
            }

            return null;
        }

        private StandingCharacterView FindViewByCharacter(string characterId)
        {
            if (standingCharacterViews == null || string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            foreach (StandingCharacterView view in standingCharacterViews)
            {
                if (view != null && string.Equals(view.CharacterId, characterId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return view;
                }
            }

            return null;
        }

        private void ResetStandingCharacters()
        {
            if (standingCharacterViews == null)
            {
                return;
            }

            foreach (StandingCharacterView view in standingCharacterViews)
            {
                if (view != null)
                {
                    view.ResetView();
                }
            }
        }

        private void SetSpeakerName(string speakerName)
        {
            if (speakerNameText != null)
            {
                speakerNameText.SetText(speakerName ?? string.Empty);
            }
        }

        private bool WasAdvanceInputPressed()
        {
            bool keyboardPressed = Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame);
            bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            return keyboardPressed || mousePressed;
        }
    }
}
