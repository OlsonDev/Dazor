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

    public string ConnectionString { get; set; }
    public string RootDirectory { get; set; }
    public AutoFromClauseMode AutoFromClause { get; set; }
    public AutoJoinClauseMode AutoJoinClause { get; set; }
    public string AutoParameterNameSuffix { get; set; }
    public GitHookMode GitHook { get; set; }
    public string DefaultSeed { get; set; }
  }
}