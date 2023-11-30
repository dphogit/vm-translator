using VMTranslator;

static void PrintUsage()
{
    Console.WriteLine("\nUsage: dotnet run <INPUT>");
    Console.WriteLine("\nINPUT:");
    Console.WriteLine("  A .vm file (extension must be specified) or the name of a directory containing one or more .vm files (no extension)\n");
}

static void ProcessCommands(Parser parser, CodeWriter codeWriter)
{
    while (parser.HasMoreCommands)
    {
        parser.Advance();

        // Only occurs if the file contains no commands.
        if (parser.CurrentCommand is null)
            break;

        // Write the vm command as a comment to help interpret the chunks of assembly code written.
        codeWriter.WriteComment(parser.CurrentCommand);

        switch (parser.CurrentCommandType)
        {
            case CommandType.Push or CommandType.Pop:
                codeWriter.WritePushPop(parser.CurrentCommandType!.Value, parser.Arg1!, parser.Arg2!.Value);
                break;
            case CommandType.Arithmetic:
                codeWriter.WriteArithmetic(parser.CurrentCommand);
                break;
            case CommandType.Label:
                codeWriter.WriteLabel(parser.Arg1!);
                break;
            case CommandType.Goto:
                codeWriter.WriteGoTo(parser.Arg1!);
                break;
            case CommandType.If:
                codeWriter.WriteIf(parser.Arg1!);
                break;
            case CommandType.Function:
                codeWriter.WriteFunction(parser.Arg1!, parser.Arg2!.Value);
                break;
            case CommandType.Return:
                codeWriter.WriteReturn();
                break;
            case CommandType.Call:
                codeWriter.WriteCall(parser.Arg1!, parser.Arg2!.Value);
                break;
        }
    }
}

static void Translate(string[] vmFiles, string outputFileName)
{
    using var outputFileStream = File.Create(outputFileName);
    CodeWriter codeWriter = new(outputFileStream);
    codeWriter.WriteInit();

    foreach (string vmFile in vmFiles)
    {
        using var inputFileStream = File.OpenRead(vmFile);
        Parser parser = new(inputFileStream);

        codeWriter.SetFileName(vmFile);
        codeWriter.WriteComment($"File: {Path.GetFileName(vmFile)}");

        ProcessCommands(parser, codeWriter);
    }

    codeWriter.Close();
}

// PROGRAM ENTRY POINT

if (args.Length != 1)
{
    PrintUsage();
    return 1;
}

string inputPath = args[0];

try
{
    if (Directory.Exists(inputPath))
    {
        string[] vmFiles = Directory.GetFiles(inputPath, "*.vm");
        string outputFileName = Path.Combine(inputPath, $"{Path.GetFileNameWithoutExtension(inputPath)}.asm");
        Translate(vmFiles, outputFileName);
        return 0;
    }

    if (File.Exists(inputPath))
    {
        string ext = Path.GetExtension(inputPath);

        if (ext != ".vm")
        {
            Console.WriteLine($"Invalid file extension: {ext}. Must be .vm");
            return 1;
        }

        string outputFileName = Path.ChangeExtension(inputPath, ".asm");
        Translate([inputPath], outputFileName);
        return 0;
    }

    Console.WriteLine($"Not found: {inputPath}");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    return 1;
}
