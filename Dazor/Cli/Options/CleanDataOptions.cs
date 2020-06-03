namespace Dazor.Cli.Options {
  internal class CleanDataOptions {
    public CleanDataOptions(bool dryRun)
      => DryRun = dryRun;

    public bool DryRun { get; }
  }
}