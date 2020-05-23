using System;
using System.Collections.Generic;
using System.Linq;
using Dazor.Cli.Commands;
using Dazor.Cli.Options;
using Dazor.Cli.Interaction;
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
      var options = new InitOptions(
        GetConnectionString(),
        GetAutoFromClause(),
        GetAutoParameterNameSuffix(),
        GetGitHook(),
        GetDefaultSeed());
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

    private IEnumerable<string>? TryGetFirstArg(string[] opts) {
      var orderedOpts = opts
        .OrderBy(opt => !opt.StartsWith("--")) // Verbose syntax first
        .ThenBy(opt => opt);
      foreach (var opt in orderedOpts) {
        if (_options.TryGetValue(opt, out var values))
          return values;
      }
      return null;
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
          return Convert.To<T>(argValues!);
        }
      } catch (Exception ex) {
        Console.Error.WriteErrorLine(ex.Message);
      }
      return Prompt.With<T>(prompt);
    }

    private T PromptWithSuggestion<T>(string prompt, string[] opts) {
      var firstArg = TryGetFirstArg(opts);
      var suggestion = firstArg is null
        ? ""
        : string.Join(' ', firstArg);
      return Prompt.WithSuggestion<T>(prompt, suggestion);
    }

    private string GetConnectionString() {
      if (TryGetArg(out var connectionString, Opts.ConnectionString)) {
        return string.Join(" ", connectionString!);
      }

      var builder = new SqlConnectionStringBuilder {
        DataSource = GetArgAndPrompt<string>("Data source (database server address)?", Opts.DataSource),
        InitialCatalog = GetArgAndPrompt<string>("Initial catalog (database name)?", Opts.Database),
      };

      var hasIntegratedSecurity = HasArg(Opts.IntegratedSecurity);
      var hasUserId = HasArg(Opts.User);
      var hasPassword = HasArg(Opts.Password);

      var ambiguousAuth = hasIntegratedSecurity && (hasUserId || hasPassword);
      if (ambiguousAuth) {
        Console.Error.WriteErrorLine($"The options {string.Join("/", Opts.IntegratedSecurity)} are mutually exclusive with {string.Join("/", Opts.User)} and {string.Join("/", Opts.Password)}.");
        hasIntegratedSecurity = false;
        hasUserId = false;
        hasPassword = false;
      }

      if (!hasUserId && !hasPassword && (hasIntegratedSecurity || Prompt.With<bool>("Integrated security?"))) {
        builder.IntegratedSecurity = true;
      } else if (ambiguousAuth) {
        builder.UserID = PromptWithSuggestion<string>("User ID?", Opts.User);
        builder.Password = PromptWithSuggestion<string>("Password?", Opts.Password);
      } else {
        builder.UserID = GetArgAndPrompt<string>("User ID?", Opts.User);
        builder.Password = GetArgAndPrompt<string>("Password?", Opts.Password);
      }

      return builder.ToString();
    }

    // TODO: Suggest "On" if not specified.
    private OffOrOn GetAutoFromClause()
      => GetArgAndPrompt<OffOrOn>("Do you want automatic FROM clause insertion?", Opts.AutoFromClause);

    // TODO: Suggest "QueryParameters" if not specified.
    private string GetAutoParameterNameSuffix()
      => GetArgAndPrompt<string>("What suffix do you want automatically stripped off your query parameters?", Opts.AutoParameterNameSuffix);

    // TODO: Suggest "On" if not specified.
    private OffOrOn GetGitHook()
      => GetArgAndPrompt<OffOrOn>("Do you want a helpful git-hook installed?", Opts.GitHook);

    // TODO: Suggest "default" if not specified.
    private string GetDefaultSeed()
      => GetArgAndPrompt<string>("What do you want the name of your default seed to be?", Opts.DefaultSeed);
  }
}