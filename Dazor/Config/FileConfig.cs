using System;

namespace Dazor.Config {
  /// <summary>Represents a dazor.json configuration file.</summary>
  public class FileConfig {
    public string? ConnectionString { get; set; }
    public AutoFromClauseMode? AutoFromClause { get; set; }
    public AutoJoinMode? AutoJoins { get; set; }
    public string? AutoParameterNameSuffix { get; set; }

    public BoundConfig BindDefaults()
      => new BoundConfig(
        ConnectionString ?? throw new InvalidOperationException("ConnectionString cannot be null."),
        AutoFromClause ?? AutoFromClauseMode.On,
        AutoJoins ?? AutoJoinMode.ForeignKey,
        AutoParameterNameSuffix ?? "QueryParameters");
  }
}