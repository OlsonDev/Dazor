namespace Dazor.Cli.Options {
  internal class UpgradeOptions {
    internal UpgradeOptions(string toVersion)
      => ToVersion = toVersion;

    internal string ToVersion { get; }
  }
}