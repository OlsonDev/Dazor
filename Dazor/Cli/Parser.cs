using System;
using System.Collections.Generic;
using System.Linq;
using Dazor.Cli.Commands;
using Dazor.Cli.Options;
using Dazor.Extensions;

namespace Dazor.Cli {
  internal class Parser {
    private readonly IDictionary<string, IEnumerable<string>> _options = new Dictionary<string, IEnumerable<string>>();
    internal string[] CommandLineArgs { get; }
    internal string Command => CommandLineArgs.Length >= 1
      ? CommandLineArgs[0].ToLowerInvariant()
      : "help";

    internal Parser(string[] args) {
      CommandLineArgs = args;
      BuildOpts();
    }

    private void BuildOpts() {
      var key = string.Empty;
      var values = new List<string>();

      foreach (var arg in CommandLineArgs.Skip(1)) {
        if (arg.StartsWith('-')) {
          _options[key] = values;
          key = arg;
          values = new List<string>();
        } else {
          values.Add(arg);
        }
      }
      _options[key] = values;
    }

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
    private ParseResult ParseInit() {
      var options = new InitOptions {
        ConnectionString = GetArg<string>("Connection string?", "--connection-string")
      };
      var command = new InitCommand(options);
      return new ParseResult { Command = command };
    }

    private ParseResult ParseMigrate() => throw new NotImplementedException();
    private ParseResult ParseNewSeed() => throw new NotImplementedException();
    private ParseResult ParseRerun() => throw new NotImplementedException();
    private ParseResult ParseUndo() => throw new NotImplementedException();
    private ParseResult ParseUpgrade() => throw new NotImplementedException();
    private ParseResult ParseWatch() => throw new NotImplementedException();

    private ParseResult ParseNonexistent()
      => new ParseResult($"dazor: '{Command}' is not a dazor command. See 'dazor help'.");

    private IEnumerable<string> GetArg(string prompt, params string[] opts) {
      foreach (var opt in opts) {
        if (_options.TryGetValue(opt, out var values)) {
          foreach (var value in values) {
            yield return value;
          }
          yield break;
        }
      }
      Console.Write(prompt.Trim() + " ");
      var input = Console.ReadLine();
      yield return input;
    }

    private T GetArg<T>(string prompt, params string[] opts) {
      var values = GetArg(prompt, opts);
      var type = typeof(T);
      if (type == typeof(string)) return (T)(object)values.Cast<string>().Single();
      throw new NotImplementedException($"{nameof(GetArg)}<{type.GetFriendlyName()}> not implemented.");
    }

    internal static ParseResult Parse(string[] args) => new Parser(args).Parse();
  }
}