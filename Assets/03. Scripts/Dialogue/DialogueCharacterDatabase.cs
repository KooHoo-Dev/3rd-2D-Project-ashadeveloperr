using System;
using UnityEngine;

namespace Babel
{
    public class DialogueCharacterDatabase : MonoBehaviour
    {
        [SerializeField] private CharacterDefinition[] characters;

        public CharacterDefinition GetCharacter(string characterId)
        {
            if (characters == null || string.IsNullOrWhiteSpace(characterId))
            {
                return null;
            }

            foreach (CharacterDefinition character in characters)
            {
                if (character != null && string.Equals(character.CharacterId, characterId, StringComparison.OrdinalIgnoreCase))
                {
                    return character;
                }
            }

            return null;
        }
    }
}
