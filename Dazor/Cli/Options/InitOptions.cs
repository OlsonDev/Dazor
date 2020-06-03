using Dazor.Config;

namespace Dazor.Cli.Options {
  internal class InitOptions {
    public InitOptions(
      string connectionString,
      string rootDirectory,
      AutoFromClauseMode autoFromClause,
      AutoJoinClauseMode autoJoinClause,
      string autoParameterNameSuffix,
      GitHookMode gitHook,
      string defaultSeed) {
      ConnectionString = connectionString;
      RootDirectory = rootDirectory;
      AutoFromClause = autoFromClause;
      AutoJoinClause = autoJoinClause;
      AutoParameterNameSuffix = autoParameterNameSuffix;
      GitHook = gitHook;
      DefaultSeed = defaultSeed;
    }

    public string ConnectionString { get; }
    public string RootDirectory { get; }
    public AutoFromClauseMode AutoFromClause { get; }
    public AutoJoinClauseMode AutoJoinClause { get; }
    public string AutoParameterNameSuffix { get; }
    public GitHookMode GitHook { get; }
    public string DefaultSeed { get; }
  }
}