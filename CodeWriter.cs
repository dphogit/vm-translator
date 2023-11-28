namespace VMTranslator;

/// <summary>
/// Responsible for translating VM commands into Hack assembly code. To ensure data is committed to the output 
/// destination, <see cref="Close"/> must be called when finished writing.
/// </summary>
/// <param name="stream">The output stream to write to.</param>
public class CodeWriter(Stream stream)
{
    private readonly StreamWriter writer = new(stream);

    // Used to generate unique labels for comparison commands
    private int labelCounter = 0;

    public void Close()
    {
        writer.Close();
    }

    public void WriteComment(string comment)
    {
        writer.WriteLine($"// {comment}");
    }

    public void WriteArithmetic(string command)
    {
        if (ArithmeticCommands.Binary.Contains(command))
        {
            PopStackToD();          // The D register contains the 'y' value.
        }

        DecrementStackPointer();
        SetAToStackPointer();       // The A register contains the 'x' value.

        switch (command)
        {
            case ArithmeticCommands.Add:
                writer.WriteLine("M=M+D");
                break;
            case ArithmeticCommands.Sub:
                writer.WriteLine("M=M-D");
                break;
            case ArithmeticCommands.And:
                writer.WriteLine("M=M&D");
                break;
            case ArithmeticCommands.Or:
                writer.WriteLine("M=M|D");
                break;
            case ArithmeticCommands.Neg:
                writer.WriteLine("M=-M");
                break;
            case ArithmeticCommands.Not:
                writer.WriteLine("M=!M");
                break;
            case ArithmeticCommands.Lt:
            case ArithmeticCommands.Eq:
            case ArithmeticCommands.Gt:
                WriteComparison(command);
                break;
        }

        IncrementStackPointer();
    }

    public void WritePushPop(CommandType commandType, string segment, int index)
    {
        if (commandType == CommandType.Push)
        {
            switch (segment)
            {
                case "constant":
                    LoadConstantIntoD(index);
                    PushDToStack();
                    break;
            }
        }
    }

    private void WriteComparison(string command)
    {
        string jumpCommand = command switch
        {
            ArithmeticCommands.Lt => "JLT",
            ArithmeticCommands.Eq => "JEQ",
            ArithmeticCommands.Gt => "JGT",
            _ => throw new InvalidOperationException($"Invalid comparison command: {command}"),
        };

        string trueLabel = $"IF_TRUE_{labelCounter}";
        string endLabel = $"END_COMP_{labelCounter}";

        // Get the comparison result
        writer.WriteLine($"D=M-D");

        // Jump if comparison is true
        writer.WriteLine($"@{trueLabel}");
        writer.WriteLine($"D;{jumpCommand}");

        // Comparison is false
        SetAToStackPointer();
        writer.WriteLine("M=0");
        writer.WriteLine($"@{endLabel}");
        writer.WriteLine("0;JMP");

        // A true comparison jumps here and writes -1 (true) to the stack
        writer.WriteLine($"({trueLabel})");
        SetAToStackPointer();
        writer.WriteLine("M=-1");

        // End of comparison - a false comparison jumps here
        writer.WriteLine($"({endLabel})");

        labelCounter++;
    }

    private void LoadConstantIntoD(int constant)
    {
        writer.WriteLine($"@{constant}");
        writer.WriteLine("D=A");
    }

    private void SetAToStackPointer()
    {
        writer.WriteLine("@SP");
        writer.WriteLine("A=M");
    }

    private void PopStackToD()
    {
        writer.WriteLine("@SP");
        writer.WriteLine("AM=M-1");
        writer.WriteLine("D=M");
    }

    private void PushDToStack()
    {
        writer.WriteLine("@SP");
        writer.WriteLine("A=M");
        writer.WriteLine("M=D");
        IncrementStackPointer();
    }

    private void IncrementStackPointer()
    {
        writer.WriteLine("@SP");
        writer.WriteLine("M=M+1");
    }

    private void DecrementStackPointer()
    {
        writer.WriteLine("@SP");
        writer.WriteLine("M=M-1");
    }
}