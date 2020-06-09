namespace Dazor.Cli.Options {
  internal class DowngradeOptions {
    internal DowngradeOptions(string toVersion)
      => ToVersion = toVersion;

    internal string ToVersion { get; }
  }
}