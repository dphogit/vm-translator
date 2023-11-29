namespace VMTranslator;

public enum CommandType
{
    Arithmetic,
    Push,
    Pop,
    Label,
    Goto,
    If,
    Function,
    Return,
    Call,
}

public static class ArithmeticCommands
{
    public const string Add = "add";
    public const string Sub = "sub";
    public const string Neg = "neg";
    public const string Eq = "eq";
    public const string Gt = "gt";
    public const string Lt = "lt";
    public const string And = "and";
    public const string Or = "or";
    public const string Not = "not";

    public static readonly HashSet<string> All = [Add, Sub, Neg, Eq, Gt, Lt, And, Or, Not];
    public static readonly HashSet<string> Unary = [Neg, Not];
    public static readonly HashSet<string> Binary = [Add, Sub, Eq, Gt, Lt, And, Or];
    public static readonly HashSet<string> Comparison = [Eq, Gt, Lt];
}

public static class StackCommands
{
    public const string Push = "push";
    public const string Pop = "pop";
}

public static class MemorySegments
{
    public const string Argument = "argument";
    public const string Local = "local";
    public const string Static = "static";
    public const string Constant = "constant";
    public const string This = "this";
    public const string That = "that";
    public const string Pointer = "pointer";
    public const string Temp = "temp";

    /// <summary>
    /// Maps the segment name to their corresponding assembly base address.
    /// </summary>
    public static readonly Dictionary<string, string> Addresses = new()
    {
        [Local] = "LCL",        // R1
        [Argument] = "ARG",     // R2
        [This] = "THIS",        // R3
        [That] = "THAT",        // R4
        [Temp] = "5",           // R5-R12
        // R13-15 are general purpose registers
        [Static] = "16",        // R16-R255
    };
}