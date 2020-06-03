using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Dazor.Config {
  internal static class Serializer {
    internal static async Task<BoundConfig> ReadAsync() {
      using var stream = File.OpenRead("dazor.json");
      var fileConfig = await JsonSerializer.DeserializeAsync<FileConfig>(stream, BuildOptions());
      return fileConfig.BindDefaults();
    }

    internal static Task WriteAsync(FileConfig config)
      => WriteAsync(config.BindDefaults());

    internal static Task WriteAsync(BoundConfig config) {
      var json = JsonSerializer.Serialize(config, BuildOptions());
      return File.WriteAllTextAsync("dazor.json", json);
    }

    private static JsonSerializerOptions BuildOptions() {
      var serializerOptions = new JsonSerializerOptions {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        AllowTrailingCommas = true,
        IgnoreNullValues = false,
        PropertyNameCaseInsensitive = true,
      };
      serializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
      return serializerOptions;
    }
  }
}