using System;
using UnityEngine;
using static Babel.TotalGameManager;

namespace Babel
{
    [Serializable]
    public class DialogueTriggerRule
    {
        [SerializeField] private string dialogueSceneId;
        [SerializeField] private DialogueTiming timing;
        [SerializeField] private DialogueResultCondition resultCondition = DialogueResultCondition.Any;
        [SerializeField] private GameMode gameMode = GameMode.Story;
        [SerializeField] private int stageLevel = 1;
        [SerializeField] private bool onceOnly = true;
        [SerializeField] private int priority;

        public string DialogueSceneId => dialogueSceneId;
        public DialogueTiming Timing => timing;
        public DialogueResultCondition ResultCondition => resultCondition;
        public GameMode GameMode => gameMode;
        public int StageLevel => stageLevel;
        public bool OnceOnly => onceOnly;
        public int Priority => priority;

        public bool Matches(DialogueTiming targetTiming, DialogueResultCondition targetResultCondition, GameMode targetGameMode, int targetStageLevel)
        {
            if (string.IsNullOrWhiteSpace(dialogueSceneId))
            {
                return false;
            }

            if (timing != targetTiming)
            {
                return false;
            }

            if (gameMode != targetGameMode)
            {
                return false;
            }

            if (stageLevel > 0 && stageLevel != targetStageLevel)
            {
                return false;
            }

            if (resultCondition != DialogueResultCondition.Any && resultCondition != targetResultCondition)
            {
                return false;
            }

            if (onceOnly && TotalGameManager.Instance.HasSeenDialogue(dialogueSceneId))
            {
                return false;
            }

            return true;
        }
    }
}
