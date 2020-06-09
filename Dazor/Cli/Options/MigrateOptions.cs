namespace Dazor.Cli.Options {
  internal class MigrateOptions {
    internal MigrateOptions(string toVersion)
      => ToVersion = toVersion;

    internal string ToVersion { get; }
  }
}