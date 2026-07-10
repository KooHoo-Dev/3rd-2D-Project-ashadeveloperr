using System;

namespace Babel
{
    [Serializable]
    public class DialogueLine
    {
        public string SceneId { get; set; }
        public int LineIndex { get; set; }
        public DialogueCommand Command { get; set; }
        public string SpeakerId { get; set; }
        public string Text { get; set; }
        public string Expression { get; set; }
        public DialogueSlot Slot { get; set; }
        public float WaitSeconds { get; set; }
    }
}
