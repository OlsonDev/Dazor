using System;
using System.Collections.Generic;
using System.Linq;
using Dazor.Cli.Commands;
using Dazor.Cli.Options;
using Dazor.Cli.Interaction;
using Dazor.Extensions;
using Microsoft.Data.SqlClient;
using Dazor.Config;
using Dazor.Cli.Opts;
using System.Threading.Tasks;

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

    internal static Task<IParseResult> ParseAsync(string[] args)
      => new Parser(args).ParseAsync();

    internal async Task<IParseResult> ParseAsync() {
      try {
        return await (Command switch
        {
          "apply-seed" => ParseApplySeedAsync(),
          "clean-data" => ParseCleanDataAsync(),
          "clean-schema" => ParseCleanSchemaAsync(),
          "downgrade" => ParseDowngradeAsync(),
          "fix-seeds" => ParseFixSeedsAsync(),
          "generate" => ParseGenerateAsync(),
          "help" => ParseHelpAsync(),
          "init" => ParseInitAsync(),
          "migrate" => ParseMigrateAsync(),
          "new-seed" => ParseNewSeedAsync(),
          "rerun" => ParseRerunAsync(),
          "undo" => ParseUndoAsync(),
          "upgrade" => ParseUpgradeAsync(),
          "watch" => ParseWatchAsync(),
          _ => ParseNonexistentAsync(),
        });
      } catch (ParseException ex) {
        return new ErrorResult(ex.Message);
      }
    }

    private Task<IParseResult> ParseApplySeedAsync() => throw new NotImplementedException();

    private async Task<IParseResult> ParseCleanDataAsync() {
      var config = await ReadConfigAsync();
      var options = new CleanDataOptions(GetDryRun());
      return new CommandResult(new CleanDataCommand(options, config));
    }

    private async Task<IParseResult> ParseCleanSchemaAsync() {
      var config = await ReadConfigAsync();
      var options = new CleanSchemaOptions(GetDryRun());
      return new CommandResult(new CleanSchemaCommand(options, config));
    }

    private Task<IParseResult> ParseDowngradeAsync() => throw new NotImplementedException();

    private Task<IParseResult> ParseFixSeedsAsync() => throw new NotImplementedException();

    private Task<IParseResult> ParseGenerateAsync() => throw new NotImplementedException();

    private Task<IParseResult> ParseHelpAsync()
      => Cmd(new HelpCommand());

    private Task<IParseResult> ParseInitAsync() {
      // TODO: Use something like System.Data.Sql.SqlDataSourceEnumerator to get
      //       network-local SQL Server data sources.
      var options = new InitOptions(
        GetConnectionString(),
        GetRootDirectory(),
        GetAutoFromClause(),
        GetAutoJoinClause(),
        GetAutoParameterNameSuffix(),
        GetGitHook(),
        GetDefaultSeed());
      return Cmd(new InitCommand(options));
    }

    private Task<IParseResult> ParseMigrateAsync() => throw new NotImplementedException();

    private Task<IParseResult> ParseNewSeedAsync() => throw new NotImplementedException();

    private Task<IParseResult> ParseRerunAsync() => throw new NotImplementedException();

    private Task<IParseResult> ParseUndoAsync() => throw new NotImplementedException();

    private async Task<IParseResult> ParseUpgradeAsync() {
      var config = await ReadConfigAsync();
      var options = new UpgradeOptions(GetToVersion());
      return new CommandResult(new UpgradeCommand(options, config, CommandLineArgs));
    }

    private Task<IParseResult> ParseWatchAsync() => throw new NotImplementedException();

    private Task<IParseResult> ParseNonexistentAsync()
      => Task.FromResult<IParseResult>(new ErrorResult($"dazor: '{Command}' is not a dazor command. See 'dazor help'."));

    private Task<IParseResult> Cmd(ICommand command)
      => Task.FromResult<IParseResult>(new CommandResult(command));

    private Task<BoundConfig> ReadConfigAsync()
      => Serializer.ReadAsync(); // TODO: Handle errors gracefully

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

    private bool GetDryRun() {
      // TODO: Optional args
      return true;
    }

    // TODO: Proper implementation with prompt if failure...
    private string GetToVersion() => _options.TryGetValue("", out var values)
      ? values.Skip(1).First()
      : throw new InvalidOperationException("Specify `to` next time...");

    private string GetConnectionString() {
      if (TryGetArg(out var connectionString, InitOpts.ConnectionString)) {
        return string.Join(" ", connectionString!);
      }

      var builder = new SqlConnectionStringBuilder {
        DataSource = GetArgAndPrompt<string>("Data source (database server address)?", InitOpts.DataSource),
        InitialCatalog = GetArgAndPrompt<string>("Initial catalog (database name)?", InitOpts.Database),
      };

      var hasIntegratedSecurity = HasArg(InitOpts.IntegratedSecurity);
      var hasUserId = HasArg(InitOpts.User);
      var hasPassword = HasArg(InitOpts.Password);

      var ambiguousAuth = hasIntegratedSecurity && (hasUserId || hasPassword);
      if (ambiguousAuth) {
        Console.Error.WriteErrorLine($"The options {string.Join("/", InitOpts.IntegratedSecurity)} are mutually exclusive with {string.Join("/", InitOpts.User)} and {string.Join("/", InitOpts.Password)}.");
        hasIntegratedSecurity = false;
        hasUserId = false;
        hasPassword = false;
      }

      if (!hasUserId && !hasPassword && (hasIntegratedSecurity || Prompt.With<bool>("Integrated security?"))) {
        builder.IntegratedSecurity = true;
      } else if (ambiguousAuth) {
        builder.UserID = PromptWithSuggestion<string>("User ID?", InitOpts.User);
        builder.Password = PromptWithSuggestion<string>("Password?", InitOpts.Password);
      } else {
        builder.UserID = GetArgAndPrompt<string>("User ID?", InitOpts.User);
        builder.Password = GetArgAndPrompt<string>("Password?", InitOpts.Password);
      }

      return builder.ToString();
    }

    // TODO: Suggest "./SQL" if not specified.
    private string GetRootDirectory()
      => GetArgAndPrompt<string>("Relative to here, where do you want your migrations and seeds stored?", InitOpts.RootDirectory);

    // TODO: Suggest "On" if not specified.
    private AutoFromClauseMode GetAutoFromClause()
      => GetArgAndPrompt<AutoFromClauseMode>("Do you want automatic FROM clause insertion?", InitOpts.AutoFromClause);

    // TODO: Suggest "ForeignKey" if not specified.
    private AutoJoinClauseMode GetAutoJoinClause()
      => GetArgAndPrompt<AutoJoinClauseMode>("How do you want incomplete JOINs to be handled?", InitOpts.AutoJoinClause);

    // TODO: Suggest "QueryParameters" if not specified.
    private string GetAutoParameterNameSuffix()
      => GetArgAndPrompt<string>("What suffix do you want automatically stripped off your query parameters?", InitOpts.AutoParameterNameSuffix);

    // TODO: Suggest "On" if not specified.
    private GitHookMode GetGitHook()
      => GetArgAndPrompt<GitHookMode>("Do you want a helpful git-hook installed?", InitOpts.GitHook);

    // TODO: Suggest "default" if not specified.
    private string GetDefaultSeed()
      => GetArgAndPrompt<string>("What do you want the name of your default seed to be?", InitOpts.DefaultSeed);
  }
}