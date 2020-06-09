namespace Dazor.Cli.Options {
  internal class RerunOptions {
    internal RerunOptions(string what)
      => What = what;

    internal string What { get; }
  }
}