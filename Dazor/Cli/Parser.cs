using System;
using System.Collections.Generic;
using System.Data;
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

    internal IParseResult Parse()
      => Command switch
      {
        "apply-seed" => ParseApplySeed(),
        "clean-data" => ParseCleanData(),
        "clean-schema" => ParseCleanSchema(),
        "downgrade" => ParseDowngrade(),
        "fix-seeds" => ParseFixSeeds(),
        "generate" => ParseGenerate(),
        "help" => ParseHelp(),
        "init" => ParseInit(),
        "migrate" => ParseMigrate(),
        "new-seed" => ParseNewSeed(),
        "rerun" => ParseRerun(),
        "undo" => ParseUndo(),
        "upgrade" => ParseUpgrade(),
        "watch" => ParseWatch(),
        _ => ParseNonexistent(),
      };

    private IParseResult ParseApplySeed() => throw new NotImplementedException();

    private IParseResult ParseCleanData() => throw new NotImplementedException();

    private IParseResult ParseCleanSchema() => throw new NotImplementedException();

    private IParseResult ParseDowngrade() => throw new NotImplementedException();

    private IParseResult ParseFixSeeds() => throw new NotImplementedException();

    private IParseResult ParseGenerate() => throw new NotImplementedException();

    private IParseResult ParseHelp() {
      var command = new HelpCommand();
      return new CommandResult(command);
    }

    private IParseResult ParseInit() {
      // TODO: Use something like System.Data.Sql.SqlDataSourceEnumerator to get
      //       network-local SQL Server data sources.
      var options = new InitOptions {
        ConnectionString = GetArg<string>("Connection string?", "--connection-string", "-cs")
      };
      var command = new InitCommand(options);
      return new CommandResult(command);
    }

    private IParseResult ParseMigrate() => throw new NotImplementedException();

    private IParseResult ParseNewSeed() => throw new NotImplementedException();

    private IParseResult ParseRerun() => throw new NotImplementedException();

    private IParseResult ParseUndo() => throw new NotImplementedException();

    private IParseResult ParseUpgrade() => throw new NotImplementedException();

    private IParseResult ParseWatch() => throw new NotImplementedException();

    private IParseResult ParseNonexistent()
      => new ErrorResult($"dazor: '{Command}' is not a dazor command. See 'dazor help'.");

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
      yield return Console.ReadLine() ?? ""; // If Ctrl+Z is pressed.
    }

    private T GetArg<T>(string prompt, params string[] opts) {
      var values = GetArg(prompt, opts);
      var type = typeof(T);
      if (type == typeof(string)) return (T)(object)values.Cast<string>().Single();
      throw new NotImplementedException($"{nameof(GetArg)}<{type.GetFriendlyName()}> not implemented.");
    }

    internal static IParseResult Parse(string[] args)
      => new Parser(args).Parse();
  }
}