namespace Dazor.Cli.Options {
  internal class UndoOptions {
    internal UndoOptions(string what)
      => What = what;

    internal string What { get; }
  }
}