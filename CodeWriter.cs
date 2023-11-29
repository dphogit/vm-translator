using System.ComponentModel;

namespace VMTranslator;

/// <summary>
/// Responsible for translating VM commands into Hack assembly code. 
/// When translating a VM file, use the <see cref="SetFileName"/> method to set the current filename to the one 
/// that is being translated. This is used to generate unique static variable names.
/// To ensure data is committed to the output destination, <see cref="Close"/> must be called when finished writing.
/// </summary>
/// <param name="stream">The output stream to write to.</param>
public class CodeWriter(Stream stream)
{
    private readonly StreamWriter writer = new(stream);

    // Used to generate unique labels for comparison commands
    private int labelCounter = 0;

    // The current .vm file that is being translated. Used to generate unique static variable names.
    private string? fileName;

    /// <summary>
    /// Sets the current .vm file that is being translated, removing the prefix path and file extension. 
    /// The filename is important for the generation of unique static variable names.
    /// </summary>
    /// <param name="fileName"></param>
    public void SetFileName(string fileName)
    {
        this.fileName = Path.GetFileNameWithoutExtension(fileName);
    }

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
                writer.WriteLine("M=D+M");
                break;
            case ArithmeticCommands.Sub:
                writer.WriteLine("M=M-D");
                break;
            case ArithmeticCommands.And:
                writer.WriteLine("M=M&D");
                break;
            case ArithmeticCommands.Or:
                writer.WriteLine("M=D|M");
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
        switch (commandType)
        {
            case CommandType.Push:
                WritePush(segment, index);
                break;
            case CommandType.Pop:
                WritePop(segment, index);
                break;
            default:
                throw new InvalidOperationException($"Invalid command type: {commandType}. Must be Push or Pop.");

        }
    }

    private void WriteComparison(string command)
    {
        string jumpCommand = command switch
        {
            ArithmeticCommands.Lt => "JLT",
            ArithmeticCommands.Eq => "JEQ",
            ArithmeticCommands.Gt => "JGT",
            _ => throw new InvalidOperationException($"Invalid comparison command: {command}. Must be lt, eq, or gt."),
        };

        string trueLabel = $"IF_TRUE_{labelCounter}";
        string endLabel = $"END_COMP_{labelCounter}";

        // Get the comparison result
        writer.WriteLine($"D=M-D");

        // Jump to true path code execution if comparison is true
        writer.WriteLine($"@{trueLabel}");
        writer.WriteLine($"D;{jumpCommand}");

        // If the comparison is false, this code will execute and jump to the false branch.
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

    private void WritePush(string segment, int index)
    {
        if (segment == MemorySegments.Constant)
        {
            LoadConstantIntoD(index);
        }
        else if (segment == MemorySegments.Static)
        {
            if (fileName is null)
                throw new InvalidOperationException("The filename must be set before writing static variables.");

            // Symbol for static variables are in the form of <filename>.<index>
            writer.WriteLine($"@{fileName}.{index}");
            writer.WriteLine("D=M");
        }
        else
        {
            string addressToACommand = segment == MemorySegments.Temp || segment == MemorySegments.Pointer
                ? "A=D+A"
                : "A=D+M";

            LoadConstantIntoD(index);
            writer.WriteLine($"@{MemorySegments.Addresses[segment]}");
            writer.WriteLine(addressToACommand);
            writer.WriteLine("D=M");
        }

        PushDToStack();
    }

    private void WritePop(string segment, int index)
    {
        if (segment == MemorySegments.Static)
        {
            if (fileName is null)
                throw new InvalidOperationException("The filename must be set before writing static variables.");

            PopStackToD();
            writer.WriteLine($"@{fileName}.{index}");
            writer.WriteLine("M=D");
            return;
        }

        // D will store what address what we want to write to
        string addressToDCommand = segment == MemorySegments.Temp || segment == MemorySegments.Pointer
            ? "D=D+A"
            : "D=D+M";

        // Store the address we want to write to in R13
        LoadConstantIntoD(index);
        writer.WriteLine($"@{MemorySegments.Addresses[segment]}");
        writer.WriteLine(addressToDCommand);
        writer.WriteLine("@R13");
        writer.WriteLine("M=D");

        // Pop the value from the stack into D and write to the address stored in R13
        PopStackToD();
        writer.WriteLine("@R13");
        writer.WriteLine("A=M");
        writer.WriteLine("M=D");
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