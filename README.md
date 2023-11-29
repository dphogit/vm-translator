# Nand2Tetris VM Translator

This repository contains an implementation of a VM translator for the Hack computer in project 07
of the [Nand2Tetris](https://www.nand2tetris.org/) course. Written in C# (.NET 8.0), the VM
translator command line program translates input Hack VM `.vm` code files into an `.asm` assembly
file.

The program satisfies the requirements of the project, successful when comparing it's outputs to the
provided files by the project. Obviously, it can be made more robust and probably implemented
better, but this is more of a learning exercise rather than software engineering focused.
If I'm feeling cute, I might do a robust implementation in a lower level language like C++ or Rust.

## 💻 Usage

The translator can be run with the `dotnet run` command. The program takes a single argument,
either a file path to a `.vm` file or a directory containing `.vm` files. If a directory is
provided, the translator will translate all `.vm` files in the directory and output a single `.asm`
file with the same name as the directory. e.g. `dir/ -> dir.asm`.

```bash
Usage: dotnet run <INPUT>
Arguments:
    INPUT  A .vm file (extension must be specified) or the name of a directory containing one or more .vm files (no extension)
```
