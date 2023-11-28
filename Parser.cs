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

    // Encapsulate into a Command class?
    public string? CurrentCommand { get; private set; }
    public string? CommandType { get; private set; }
    public string? Arg1 { get; private set; }
    public int? Arg2 { get; private set; }

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
            CommandType = DetermineCommandType(line);
            Arg1 = DetermineArg1(line, CommandType);
            Arg2 = DetermineArg2(line, CommandType);
            return;
        }
    }

    private static string DetermineCommandType(string command)
    {
        if (command is null)
            throw new InvalidOperationException("No command has been read yet.");

        string[] parts = command.Split(" ");

        return parts[0] switch
        {
            "add" or "sub" or "neg" or "eq" or "gt" or "lt" or "and" or "or" or "not" => "C_ARITHMETIC",
            "push" => "C_PUSH",
            _ => throw new InvalidOperationException($"Invalid command type: {parts[0]}"),
        };
    }

    private static string DetermineArg1(string command, string commandType)
    {
        if (commandType == "C_ARITHMETIC")
            return command;

        string[] parts = command.Split(" ");
        return parts[1];
    }

    private static int? DetermineArg2(string command, string commandType)
    {
        if (commandType != "C_PUSH" && commandType != "C_POP" && commandType != "C_FUNCTION" && commandType != "C_CALL")
            return null;

        string[] parts = command.Split(" ");
        return int.Parse(parts[2]);
    }
}