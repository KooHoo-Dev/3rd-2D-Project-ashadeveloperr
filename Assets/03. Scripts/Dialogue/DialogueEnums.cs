namespace Babel
{
    public enum DialogueTiming
    {
        BeforeStageStart,
        BeforeResult,
    }

    public enum DialogueResultCondition
    {
        Any,
        Clear,
        Fail,
    }

    public enum DialogueCommand
    {
        Speak,
        Enter,
        Exit,
        Focus,
        Wait,
    }

    public enum DialogueSlot
    {
        L1,
        L2,
        L3,
        R1,
        R2,
        R3,
    }
}
