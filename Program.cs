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

using var filestream = File.OpenRead(filename);
var parser = new Parser(filestream);

Console.WriteLine($"Reading {filename}...");
while (parser.HasMoreCommands)
{
    parser.Advance();
    Console.WriteLine($"{parser.CurrentCommand}\t{parser.CommandType}\t{parser.Arg1}\t{parser.Arg2}\n");
}

return 0;
