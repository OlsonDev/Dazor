using Dazor.Config;

namespace Dazor.Cli.Options {
  internal class InitOptions {
    public InitOptions(
      string connectionString,
      AutoFromClauseMode autoFromClause,
      AutoJoinClauseMode autoJoinMode,
      string autoParameterNameSuffix,
      GitHookMode gitHook,
      string defaultSeed) {
      ConnectionString = connectionString;
      AutoFromClause = autoFromClause;
      AutoJoinClause = autoJoinMode;
      AutoParameterNameSuffix = autoParameterNameSuffix;
      GitHook = gitHook;
      DefaultSeed = defaultSeed;
    }

    public string ConnectionString { get; set; }
    public AutoFromClauseMode AutoFromClause { get; set; }
    public AutoJoinClauseMode AutoJoinClause { get; set; }
    public string AutoParameterNameSuffix { get; set; }
    public GitHookMode GitHook { get; set; }
    public string DefaultSeed { get; set; }
  }
}