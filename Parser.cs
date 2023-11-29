namespace VMTranslator;

/// <summary>
/// Encapsulates access to the input code. Reads a VM command, parses it, and provides convenient access to its
/// components. In addition, removes all white space and comments.
/// </summary>
/// <param name="stream">The stream to read from.</param>
public class Parser(Stream stream)
{
    private readonly StreamReader reader = new(stream);

    public bool HasMoreCommands => !reader.EndOfStream;

    public string? CurrentCommand { get; private set; }
    public CommandType? CurrentCommandType { get; private set; }
    public string? Arg1 { get; private set; }
    public int? Arg2 { get; private set; }

    private static readonly HashSet<CommandType> CommandTypesRequiringArg2 = [
        CommandType.Push,
        CommandType.Pop,
        CommandType.Function,
        CommandType.Call,
    ];

    public void Advance()
    {
        // Grabs the first line that isn't purely whitespace or a comment (also handles inline comments)
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            line = line.Split("//")[0].Trim();

            if (line == string.Empty)
                continue;

            CurrentCommand = line;
            CurrentCommandType = DetermineCommandType(line);
            Arg1 = DetermineArg1(line, CurrentCommandType.Value);
            Arg2 = DetermineArg2(line, CurrentCommandType.Value);
            return;
        }
    }

    private static CommandType DetermineCommandType(string command)
    {
        if (command is null)
            throw new InvalidOperationException("No command has been read yet.");

        string[] parts = command.Split(" ");

        return parts[0] switch
        {
            var cmd when ArithmeticCommands.All.Contains(cmd) => CommandType.Arithmetic,
            StackCommands.Push => CommandType.Push,
            StackCommands.Pop => CommandType.Pop,
            ControlFlowCommands.Label => CommandType.Label,
            ControlFlowCommands.Goto => CommandType.Goto,
            ControlFlowCommands.If => CommandType.If,
            _ => throw new InvalidOperationException($"Invalid command type: {parts[0]}"),
        };
    }

    private static string DetermineArg1(string command, CommandType commandType)
    {
        if (commandType == CommandType.Arithmetic)
            return command;

        string[] parts = command.Split(" ");
        return parts[1];
    }

    private static int? DetermineArg2(string command, CommandType commandType)
    {
        if (!CommandTypesRequiringArg2.Contains(commandType))
            return null;

        string[] parts = command.Split(" ");
        return int.Parse(parts[2]);
    }
}
