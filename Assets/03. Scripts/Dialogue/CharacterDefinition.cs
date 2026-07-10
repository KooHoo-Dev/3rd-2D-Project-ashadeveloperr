using System;
using UnityEngine;

namespace Babel
{
    [CreateAssetMenu(menuName = "Babel/Dialogue/Character Definition")]
    public class CharacterDefinition : ScriptableObject
    {
        [SerializeField] private string characterId;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private CharacterExpression[] expressions;

        public string CharacterId => characterId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? characterId : displayName;
        public Sprite DefaultSprite => defaultSprite;

        public Sprite GetSprite(string expressionId)
        {
            if (expressions != null && string.IsNullOrWhiteSpace(expressionId) == false)
            {
                foreach (CharacterExpression expression in expressions)
                {
                    if (expression != null && string.Equals(expression.ExpressionId, expressionId, StringComparison.OrdinalIgnoreCase))
                    {
                        return expression.Sprite != null ? expression.Sprite : defaultSprite;
                    }
                }
            }

            return defaultSprite;
        }
    }

    [Serializable]
    public class CharacterExpression
    {
        [SerializeField] private string expressionId;
        [SerializeField] private Sprite sprite;

        public string ExpressionId => expressionId;
        public Sprite Sprite => sprite;
    }
}
