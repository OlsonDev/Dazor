namespace Dazor.Config {
  /// <summary>Represents a dazor.json configuration file with defaults applied.</summary>
  public class BoundConfig {
    internal BoundConfig(
      string connectionString,
      AutoFromClauseMode autoFromClauseMode,
      AutoJoinMode autoJoinMode,
      string autoParameterNameSuffix) {
      ConnectionString = connectionString;
      AutoFromClauseMode = autoFromClauseMode;
      AutoJoinMode = autoJoinMode;
      AutoParameterNameSuffix = autoParameterNameSuffix;
    }
    public string ConnectionString { get; }
    public AutoFromClauseMode AutoFromClauseMode { get; }
    public AutoJoinMode AutoJoinMode { get; }
    public string AutoParameterNameSuffix { get; }
  }
}