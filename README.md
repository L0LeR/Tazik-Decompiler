# TazikDecompiler

**Pretty useless, but at the same time maybe useful** if you want to get `.cs` files from C# compiled `.dll` files.  
(Expect obfuscated ones to give you messy code — same classes, but spaghetti inside.)

> No magic here. Just [ICSharpCode.Decompiler](https://github.com/icsharpcode/ILSpy) doing the job — I just made it easier for a normal user.

## Usage

- Select a `.dll` file
- Press **Reverse**
- Get your `.cs` files in a `decompiled` folder next to the DLL (by default)

## Notes

- Obfuscated DLLs produce messy code
- Dark theme included, because light mode is pain
- It's a test build, so expect bugs and weird behaviour

## Open Source

This project is **open source**. You're free to fork it, change it, improve it, or break it.  
Pull requests are welcome if you actually make something better!

## Security

**This is important.**

- **Always build from the official source code.**
- **Never trust pre-compiled binaries from forks or random sites — they may contain malware.**
- **The safest way is to clone this repo and build it yourself in Visual Studio.**
- **That way you know exactly what you're running.**

Don't be that person who grabs a random version of Tazik Decompiler from the un-offical github repo or sites and wonders why their PC is mining crypto. Build it yourself.
