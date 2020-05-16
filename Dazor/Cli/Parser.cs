using System;
using System.Collections.Generic;
using System.Linq;
using Dazor.Cli.Commands;
using Dazor.Cli.Options;
using Dazor.Extensions;
using Microsoft.Data.SqlClient;

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

    internal static IParseResult Parse(string[] args)
      => new Parser(args).Parse();

    internal IParseResult Parse() {
      try {
        return Command switch
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
      } catch (ParseException ex) {
        return new ErrorResult(ex.Message);
      }
    }

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
        ConnectionString = GetConnectionString()
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

    private bool HasArg(string[] opts) {
      var optsFound = new HashSet<string>();
      foreach (var opt in opts) {
        if (_options.ContainsKey(opt)) optsFound.Add(opt);
      }

      if (optsFound.Count > 1) {
        throw new ParseException($"The options {opts.ToFriendlyList()} are mutually exclusive.");
      }

      return optsFound.Count == 1;
    }

    private bool TryGetArg(out IEnumerable<string>? arg, string[] opts) {
      arg = null;

      var optsFound = new HashSet<string>();
      foreach (var opt in opts) {
        if (_options.TryGetValue(opt, out var values)) {
          if (optsFound.Count == 0) {
            arg = values;
          }
          optsFound.Add(opt);
        }
      }

      if (optsFound.Count > 1) {
        // TODO: If mutually exclusive options are passed, make the user pick one or re-type.
        throw new ParseException($"The options {opts.ToFriendlyList()} are mutually exclusive.");
      }

      return optsFound.Count == 1;
    }

    private T GetArgAndPrompt<T>(string prompt, string[] opts) {
      try {
        if (TryGetArg(out var argValues, opts)) {
          return ConvertArg<T>(argValues!);
        }
      } catch (Exception ex) {
        Console.Error.WriteErrorLine(ex.Message);
      }
      return Prompt<T>(prompt);
    }

    private T Prompt<T>(string prompt) {
      prompt = AdjustPromptByType<T>(prompt);
      while (true) {
        try {
          return ConvertArg<T>(Prompt(prompt));
        } catch (Exception ex) {
          Console.Error.WriteErrorLine(ex.Message);
        }
      }
    }

    private IEnumerable<string> Prompt(string prompt) {
      var previousColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.Write(prompt.Trim() + " ");
      Console.ForegroundColor = ConsoleColor.White;
      var value = Console.ReadLine() ?? ""; // If Ctrl+Z is pressed.
      Console.ForegroundColor = previousColor;
      yield return value;
    }

    private string AdjustPromptByType<T>(string prompt) {
      var type = typeof(T);
      if (type == typeof(bool)) return prompt + " (Y/N)";
      return prompt;
    }

    private T ConvertArg<T>(IEnumerable<string> argValues) {
      var type = typeof(T);
      if (type == typeof(string)) return (T)(object)string.Join(" ", argValues);
      if (type == typeof(bool)) return (T)(object)ConvertToBool(argValues);
      throw new NotImplementedException($"{nameof(ConvertArg)}<{type.GetFriendlyName()}> not implemented.");
    }

    private bool ConvertToBool(IEnumerable<string> values)
      => values.Single().ToLowerInvariant() switch {
        "y" => true,
        "yes" => true,
        "1" => true,
        "true" => true,
        "n" => false,
        "no" => false,
        "0" => false,
        "false" => false,
        _ => throw new ParseException($"Please enter y/yes/1/true/n/no/0/false."),
      };

    private string GetConnectionString() {
      if (TryGetArg(out var connectionString, Opts.ConnectionString)) {
        return string.Join(" ", connectionString!);
      }

      var builder = new SqlConnectionStringBuilder {
        DataSource = GetArgAndPrompt<string>("DataSource?", Opts.DataSource),
        InitialCatalog = GetArgAndPrompt<string>("Initial catalog?", Opts.Database),
      };

      var hasIntegratedSecurity = HasArg(Opts.IntegratedSecurity);
      var hasUserId = HasArg(Opts.User);
      var hasPassword = HasArg(Opts.Password);

      if (hasIntegratedSecurity && (hasUserId || hasPassword)) {
        // TODO: Handle this better.
        throw new ParseException($"The options {string.Join("/", Opts.IntegratedSecurity)} are mutually exclusive with {string.Join("/", Opts.User)} and {string.Join("/", Opts.Password)}.");
      }

      if (!hasUserId && !hasPassword && (hasIntegratedSecurity || Prompt<bool>("Integrated security?"))) {
        builder.IntegratedSecurity = true;
      } else {
        builder.UserID = GetArgAndPrompt<string>("User ID?", Opts.User);
        builder.Password = GetArgAndPrompt<string>("Password?", Opts.Password);
      }

      return builder.ToString();
    }
  }
}