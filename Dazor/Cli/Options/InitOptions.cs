namespace Dazor.Cli.Options {
  internal class InitOptions {
    public InitOptions(
      string connectionString,
      OffOrOn autoFromClause,
      string autoParameterNameSuffix,
      OffOrOn gitHook,
      string defaultSeed) {
      ConnectionString = connectionString;
      AutoFromClause = autoFromClause;
      AutoParameterNameSuffix = autoParameterNameSuffix;
      GitHook = gitHook;
      DefaultSeed = defaultSeed;
    }

    public string ConnectionString { get; set; }
    public OffOrOn AutoFromClause { get; set; }
    public string AutoParameterNameSuffix { get; set; }
    public OffOrOn GitHook { get; set; }
    public string DefaultSeed { get; set; }
  }
}