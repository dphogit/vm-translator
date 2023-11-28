using VMTranslator;

static void PrintUsage()
{
    Console.WriteLine("\nUsage: dotnet run <INPUT>");
    Console.WriteLine("\nINPUT:");
    Console.WriteLine("  A .vm file (extension must be specified) or the name of a directory containing one or more .vm files (no extension)\n");
}

if (args.Length != 1)
{
    PrintUsage();
    return 1;
}

// TODO Handle directories

string filename = args[0];

if (!File.Exists(filename))
{
    Console.WriteLine($"File not found: {filename}");
    return 1;
}

if (Path.GetExtension(filename) != ".vm")
{
    Console.WriteLine($"Invalid file extension: {Path.GetExtension(filename)}. Must be .vm");
    return 1;
}

using var inputFileStream = File.OpenRead(filename);
Parser parser = new(inputFileStream);

string outputFileName = Path.ChangeExtension(filename, ".asm");
using var outputFileStream = File.OpenWrite(outputFileName);
CodeWriter codeWriter = new(outputFileStream);

try
{

    while (parser.HasMoreCommands)
    {
        parser.Advance();

        if (parser.CurrentCommand is null)
            // This should only happen if the file contains no commands.
            break;

        // Write the vm command as a comment to help interpret the chunks of assembly code written.
        codeWriter.WriteComment(parser.CurrentCommand);

        switch (parser.CommandType)
        {
            case "C_PUSH" or "C_POP":
                codeWriter.WritePushPop(parser.CommandType, parser.Arg1!, parser.Arg2!.Value);
                break;
            case "C_ARITHMETIC":
                codeWriter.WriteArithmetic(parser.CurrentCommand);
                break;
        }
    }

}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return 1;
}
finally
{
    codeWriter.Close();
}

return 0;
