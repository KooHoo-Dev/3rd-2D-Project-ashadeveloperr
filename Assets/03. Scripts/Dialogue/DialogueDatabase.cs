using System;
using System.Collections.Generic;
using UnityEngine;

namespace Babel
{
    public class DialogueDatabase : MonoBehaviour
    {
        [SerializeField] private TextAsset dialogueCsv;
        [SerializeField] private bool loadOnAwake = true;

        private readonly Dictionary<string, List<DialogueLine>> linesBySceneId = new Dictionary<string, List<DialogueLine>>();
        private bool isLoaded;

        private void Awake()
        {
            if (loadOnAwake)
            {
                Load();
            }
        }

        public IReadOnlyList<DialogueLine> GetLines(string sceneId)
        {
            if (isLoaded == false)
            {
                Load();
            }

            if (string.IsNullOrWhiteSpace(sceneId) || linesBySceneId.TryGetValue(sceneId, out List<DialogueLine> lines) == false)
            {
                return new DialogueLine[0];
            }

            return lines;
        }

        public void Load()
        {
            linesBySceneId.Clear();
            isLoaded = true;

            if (dialogueCsv == null || string.IsNullOrWhiteSpace(dialogueCsv.text))
            {
                return;
            }

            string[] rows = dialogueCsv.text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

            if (rows.Length <= 1)
            {
                return;
            }

            Dictionary<string, int> headerMap = BuildHeaderMap(ParseCsvRow(rows[0]));

            for (int i = 1; i < rows.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(rows[i]))
                {
                    continue;
                }

                List<string> columns = ParseCsvRow(rows[i]);
                DialogueLine line = CreateLine(columns, headerMap);

                if (line == null || string.IsNullOrWhiteSpace(line.SceneId))
                {
                    continue;
                }

                if (linesBySceneId.TryGetValue(line.SceneId, out List<DialogueLine> sceneLines) == false)
                {
                    sceneLines = new List<DialogueLine>();
                    linesBySceneId.Add(line.SceneId, sceneLines);
                }

                sceneLines.Add(line);
            }

            foreach (List<DialogueLine> sceneLines in linesBySceneId.Values)
            {
                sceneLines.Sort((a, b) => a.LineIndex.CompareTo(b.LineIndex));
            }
        }

        private static Dictionary<string, int> BuildHeaderMap(List<string> headers)
        {
            Dictionary<string, int> headerMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < headers.Count; i++)
            {
                string key = headers[i].Trim();

                if (headerMap.ContainsKey(key) == false)
                {
                    headerMap.Add(key, i);
                }
            }

            return headerMap;
        }

        private static DialogueLine CreateLine(List<string> columns, Dictionary<string, int> headerMap)
        {
            string sceneId = GetColumn(columns, headerMap, "sceneId");
            string commandValue = GetColumn(columns, headerMap, "command");
            string speakerId = GetColumn(columns, headerMap, "speakerId");
            string expression = GetColumn(columns, headerMap, "expression");
            string slotValue = GetColumn(columns, headerMap, "slot");
            string text = GetColumn(columns, headerMap, "text");
            string waitSecondsValue = GetColumn(columns, headerMap, "waitSeconds");
            int lineIndex = ParseInt(GetColumn(columns, headerMap, "lineIndex"), 0);
            DialogueCommand command = ParseEnum(commandValue, DialogueCommand.Speak);
            DialogueSlot slot = ParseEnum(slotValue, DialogueSlot.L1);
            float waitSeconds = ParseFloat(waitSecondsValue, 0.0f);

            return new DialogueLine
            {
                SceneId = sceneId,
                LineIndex = lineIndex,
                Command = command,
                SpeakerId = speakerId,
                Text = text,
                Expression = expression,
                Slot = slot,
                WaitSeconds = waitSeconds,
            };
        }

        private static string GetColumn(List<string> columns, Dictionary<string, int> headerMap, string header)
        {
            if (headerMap.TryGetValue(header, out int index) == false || index < 0 || index >= columns.Count)
            {
                return string.Empty;
            }

            return columns[index].Trim();
        }

        private static int ParseInt(string value, int fallback)
        {
            return int.TryParse(value, out int result) ? result : fallback;
        }

        private static float ParseFloat(string value, float fallback)
        {
            return float.TryParse(value, out float result) ? result : fallback;
        }

        private static T ParseEnum<T>(string value, T fallback) where T : struct
        {
            return Enum.TryParse(value, true, out T result) ? result : fallback;
        }

        private static List<string> ParseCsvRow(string row)
        {
            List<string> values = new List<string>();
            bool insideQuotes = false;
            string currentValue = string.Empty;

            for (int i = 0; i < row.Length; i++)
            {
                char currentChar = row[i];

                if (currentChar == '"')
                {
                    if (insideQuotes && i + 1 < row.Length && row[i + 1] == '"')
                    {
                        currentValue += '"';
                        i++;
                    }
                    else
                    {
                        insideQuotes = !insideQuotes;
                    }
                }
                else if (currentChar == ',' && insideQuotes == false)
                {
                    values.Add(currentValue);
                    currentValue = string.Empty;
                }
                else
                {
                    currentValue += currentChar;
                }
            }

            values.Add(currentValue);
            return values;
        }
    }
}
