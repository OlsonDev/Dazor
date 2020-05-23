using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Dazor.Config {
  /// <summary>Represents a dazor.json configuration file with defaults applied.</summary>
  public class BoundConfig {
    internal BoundConfig(
      string connectionString,
      string rootDirectory,
      AutoFromClauseMode autoFromClauseMode,
      AutoJoinClauseMode autoJoinClauseMode,
      string autoParameterNameSuffix,
      GitHookMode gitHookMode,
      string defaultSeed) {
      ConnectionString = connectionString;
      RootDirectory = rootDirectory;
      AutoFromClauseMode = autoFromClauseMode;
      AutoJoinClauseMode = autoJoinClauseMode;
      AutoParameterNameSuffix = autoParameterNameSuffix;
      GitHookMode = gitHookMode;
      DefaultSeed = defaultSeed;
    }
    public string ConnectionString { get; set; }
    public string RootDirectory { get; set; }
    public AutoFromClauseMode AutoFromClauseMode { get; set; }
    public AutoJoinClauseMode AutoJoinClauseMode { get; set; }
    public string AutoParameterNameSuffix { get; set; }
    public GitHookMode GitHookMode { get; set; }
    public string DefaultSeed { get; set; }

    internal Task WriteAsync() {
      var serializerOptions = new JsonSerializerOptions {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowTrailingCommas = true,
        IgnoreNullValues = false,
        PropertyNameCaseInsensitive = true,
      };
      serializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
      var json = JsonSerializer.Serialize(this, serializerOptions);
      return File.WriteAllTextAsync("dazor.json", json);
    }
  }
}