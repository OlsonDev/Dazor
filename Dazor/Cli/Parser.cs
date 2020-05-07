using System;
using System.Linq;

namespace Dazor.Cli {
  internal class Parser {
    internal string[] CommandLineArgs { get; }
    internal string Command => CommandLineArgs.Length >= 1
      ? CommandLineArgs[0].ToLowerInvariant()
      : "help";

    internal Parser(string[] args)
      => CommandLineArgs = args;

    internal ParseResult Parse()
      => Command switch
      {
        "apply-seed" => ParseApplySeed(),
        "clean-data" => ParseCleanData(),
        "clean-schema" => ParseCleanSchema(),
        "downgrade" => ParseDowngrade(),
        "fix-seeds" => ParseFixSeeds(),
        "generate" => ParseGenerate(),
        "init" => ParseInit(),
        "migrate" => ParseMigrate(),
        "new-seed" => ParseNewSeed(),
        "rerun" => ParseRerun(),
        "undo" => ParseUndo(),
        "upgrade" => ParseUpgrade(),
        "watch" => ParseWatch(),
        _ => ParseNonexistent(),
      };

    private ParseResult ParseApplySeed() => throw new NotImplementedException();
    private ParseResult ParseCleanData() => throw new NotImplementedException();
    private ParseResult ParseCleanSchema() => throw new NotImplementedException();
    private ParseResult ParseDowngrade() => throw new NotImplementedException();
    private ParseResult ParseFixSeeds() => throw new NotImplementedException();
    private ParseResult ParseGenerate() => throw new NotImplementedException();
    private ParseResult ParseInit() => throw new NotImplementedException();
    private ParseResult ParseMigrate() => throw new NotImplementedException();
    private ParseResult ParseNewSeed() => throw new NotImplementedException();
    private ParseResult ParseRerun() => throw new NotImplementedException();
    private ParseResult ParseUndo() => throw new NotImplementedException();
    private ParseResult ParseUpgrade() => throw new NotImplementedException();
    private ParseResult ParseWatch() => throw new NotImplementedException();

    private ParseResult ParseNonexistent()
      => new ParseResult($"Command `{Command}` does not exist.");

    internal static ParseResult Parse(string[] args) => new Parser(args).Parse();
  }
}