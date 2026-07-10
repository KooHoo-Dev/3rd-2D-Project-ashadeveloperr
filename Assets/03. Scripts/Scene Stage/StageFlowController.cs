using System.Collections;
using UnityEngine;

namespace Babel
{
    public class StageFlowController : SingletonBase<StageFlowController>
    {
        private GameManager gameManager;
        private UIManager uiManager;
        private DialogueManager dialogueManager;
        private DialogueTriggerDatabase triggerDatabase;
        private Coroutine stageRoutine;
        private Coroutine resultRoutine;

        public StageState CurrentState { get; private set; } = StageState.Boot;
        public bool IsGameplayBlocked => CurrentState != StageState.Playing;

        public enum StageState
        {
            Boot,
            PreDialogue,
            Tutorial,
            Playing,
            Paused,
            BeforeResultDialogue,
            GameOverEffect,
            Result,
            Transition,
        }

        public void Register(GameManager newGameManager, UIManager newUIManager)
        {
            gameManager = newGameManager;
            uiManager = newUIManager != null ? newUIManager : FindAnyObjectByType<UIManager>();
            dialogueManager = FindAnyObjectByType<DialogueManager>();
            triggerDatabase = FindAnyObjectByType<DialogueTriggerDatabase>();

            if (stageRoutine != null)
            {
                StopCoroutine(stageRoutine);
            }

            stageRoutine = StartCoroutine(StageStartRoutine());
        }

        public void HandleGameOver(GameManager targetGameManager)
        {
            if (resultRoutine != null)
            {
                return;
            }

            gameManager = targetGameManager;
            resultRoutine = StartCoroutine(ResultRoutine());
        }

        public bool TryPause()
        {
            if (CurrentState != StageState.Playing)
            {
                return false;
            }

            CurrentState = StageState.Paused;
            Time.timeScale = 0.0f;
            return true;
        }

        public void ResumePause()
        {
            if (CurrentState != StageState.Paused)
            {
                return;
            }

            CurrentState = StageState.Playing;
            Time.timeScale = 1.0f;
        }

        private IEnumerator StageStartRoutine()
        {
            CurrentState = StageState.Boot;
            Time.timeScale = 0.0f;

            yield return null;

            if (uiManager == null)
            {
                uiManager = FindAnyObjectByType<UIManager>();
            }

            if (TotalGameManager.Instance.IsStoryMode)
            {
                DialogueTriggerRule preDialogueRule = FindDialogueRule(DialogueTiming.BeforeStageStart, DialogueResultCondition.Any);

                if (preDialogueRule != null)
                {
                    CurrentState = StageState.PreDialogue;
                    yield return PlayDialogue(preDialogueRule.DialogueSceneId);
                }
            }

            if (uiManager != null && uiManager.TryBeginTutorialSplash())
            {
                CurrentState = StageState.Tutorial;

                while (uiManager.IsTutorialSplashActive)
                {
                    yield return null;
                }
            }

            CurrentState = StageState.Playing;
            Time.timeScale = 1.0f;

            if (gameManager != null)
            {
                gameManager.BeginStage();
            }

            stageRoutine = null;
        }

        private IEnumerator ResultRoutine()
        {
            CurrentState = StageState.BeforeResultDialogue;
            Time.timeScale = 0.0f;

            if (TotalGameManager.Instance.IsStoryMode && gameManager != null)
            {
                DialogueResultCondition resultCondition = gameManager.IsFailed ? DialogueResultCondition.Fail : DialogueResultCondition.Clear;
                DialogueTriggerRule beforeResultRule = FindDialogueRule(DialogueTiming.BeforeResult, resultCondition);

                if (beforeResultRule != null)
                {
                    yield return PlayDialogue(beforeResultRule.DialogueSceneId);
                }
            }

            CurrentState = StageState.GameOverEffect;
            Time.timeScale = 1.0f;

            if (gameManager != null && gameManager.IsFailed)
            {
                yield return gameManager.PlayGameOverEffectRoutine();
            }

            CurrentState = StageState.Result;

            if (gameManager != null)
            {
                yield return gameManager.FinalizeGameOverRoutine();
            }

            resultRoutine = null;
        }

        private DialogueTriggerRule FindDialogueRule(DialogueTiming timing, DialogueResultCondition resultCondition)
        {
            if (triggerDatabase == null)
            {
                triggerDatabase = FindAnyObjectByType<DialogueTriggerDatabase>();
            }

            if (triggerDatabase == null)
            {
                return null;
            }

            return triggerDatabase.FindRule(timing, resultCondition, TotalGameManager.Instance.CurrentGameMode, TotalGameManager.Instance.CurrentStageLevel);
        }

        private IEnumerator PlayDialogue(string dialogueSceneId)
        {
            if (dialogueManager == null)
            {
                dialogueManager = FindAnyObjectByType<DialogueManager>();
            }

            if (dialogueManager == null)
            {
                yield break;
            }

            yield return dialogueManager.PlayDialogue(dialogueSceneId);
        }
    }
}
