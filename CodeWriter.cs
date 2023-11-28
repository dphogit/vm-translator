namespace VMTranslator;

/// <summary>
/// Responsible for translating VM commands into Hack assembly code. To ensure data is committed to the output 
/// destination, <see cref="Close"/> must be called when finished writing.
/// </summary>
/// <param name="stream">The output stream to write to.</param>
public class CodeWriter(Stream stream)
{
    private readonly StreamWriter writer = new(stream);

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
        switch (command)
        {
            case "add":
                PopStackToD();
                DecrementStackPointer();
                SetAToStackPointer();
                writer.WriteLine("M=D+M");
                IncrementStackPointer();
                break;
        }
    }

    public void WritePushPop(string command, string segment, int index)
    {
        if (command == "C_PUSH")
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