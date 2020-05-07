using Dazor.Cli;

namespace Dazor {
  internal class Program {
    internal static int Main(string[] args)
      => (int)Parser.Parse(args).Run();
  }
}