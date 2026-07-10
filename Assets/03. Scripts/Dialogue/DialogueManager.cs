using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Babel
{
    public class DialogueManager : SingletonBase<DialogueManager>
    {
        [SerializeField] private DialogueDatabase dialogueDatabase;
        [SerializeField] private DialogueCharacterDatabase characterDatabase;
        [SerializeField] private DialogueUIController dialogueUIController;

        protected override void OnInitialize()
        {
            ResolveReferences();
        }

        public IEnumerator PlayDialogue(string sceneId)
        {
            if (string.IsNullOrWhiteSpace(sceneId))
            {
                yield break;
            }

            if (TotalGameManager.Instance.HasSeenDialogue(sceneId))
            {
                yield break;
            }

            ResolveReferences();

            if (dialogueDatabase == null || dialogueUIController == null || dialogueUIController.HasRequiredReferences == false)
            {
                Debug.LogWarning($"[DialogueManager] '{sceneId}' 다이얼로그를 실행할 수 없습니다. DialogueDatabase 또는 DialogueUIController 참조를 확인하세요.");
                yield break;
            }

            IReadOnlyList<DialogueLine> lines = dialogueDatabase.GetLines(sceneId);

            if (lines == null || lines.Count == 0)
            {
                Debug.LogWarning($"[DialogueManager] '{sceneId}'에 해당하는 CSV 대사를 찾을 수 없습니다.");
                yield break;
            }

            yield return dialogueUIController.Show();

            foreach (DialogueLine line in lines)
            {
                if (dialogueUIController.SkipConfirmed)
                {
                    break;
                }

                yield return ExecuteLine(line);
            }

            yield return dialogueUIController.Hide();
            TotalGameManager.Instance.MarkDialogueSeen(sceneId);
        }

        private IEnumerator ExecuteLine(DialogueLine line)
        {
            CharacterDefinition character = characterDatabase != null ? characterDatabase.GetCharacter(line.SpeakerId) : null;
            Sprite sprite = character != null ? character.GetSprite(line.Expression) : null;

            switch (line.Command)
            {
                case DialogueCommand.Enter:
                    yield return dialogueUIController.EnterCharacter(line, character, sprite);
                    break;
                case DialogueCommand.Exit:
                    yield return dialogueUIController.ExitCharacter(line);
                    break;
                case DialogueCommand.Focus:
                    dialogueUIController.FocusSpeaker(line.SpeakerId);
                    break;
                case DialogueCommand.Wait:
                    yield return WaitUnscaled(line.WaitSeconds);
                    break;
                case DialogueCommand.Speak:
                default:
                    yield return dialogueUIController.Speak(line, character, sprite);
                    break;
            }
        }

        private IEnumerator WaitUnscaled(float seconds)
        {
            float timer = 0.0f;

            while (timer < seconds)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private void ResolveReferences()
        {
            if (dialogueDatabase == null)
            {
                dialogueDatabase = FindAnyObjectByType<DialogueDatabase>();
            }

            if (characterDatabase == null)
            {
                characterDatabase = FindAnyObjectByType<DialogueCharacterDatabase>();
            }

            if (dialogueUIController == null)
            {
                dialogueUIController = FindAnyObjectByType<DialogueUIController>();
            }
        }
    }
}
