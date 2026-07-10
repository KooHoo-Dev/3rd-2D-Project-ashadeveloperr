using UnityEngine;
using static Babel.TotalGameManager;

namespace Babel
{
    public class DialogueTriggerDatabase : MonoBehaviour
    {
        [SerializeField] private DialogueTriggerRule[] rules;

        public DialogueTriggerRule FindRule(DialogueTiming timing, DialogueResultCondition resultCondition, GameMode gameMode, int stageLevel)
        {
            DialogueTriggerRule selectedRule = null;

            if (rules == null)
            {
                return null;
            }

            foreach (DialogueTriggerRule rule in rules)
            {
                if (rule == null || rule.Matches(timing, resultCondition, gameMode, stageLevel) == false)
                {
                    continue;
                }

                if (selectedRule == null || rule.Priority > selectedRule.Priority)
                {
                    selectedRule = rule;
                }
            }

            return selectedRule;
        }
    }
}
