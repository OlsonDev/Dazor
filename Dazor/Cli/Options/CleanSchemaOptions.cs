namespace Dazor.Cli.Options {
  internal class CleanSchemaOptions {
    public CleanSchemaOptions(bool dryRun)
      => DryRun = dryRun;

    public bool DryRun { get; }
  }
}