using System.Threading.Tasks;
using Dazor.Cli;

namespace Dazor {
  internal class Program {
    internal static async Task<int> Main(string[] args)
      => (int)await Parser.Parse(args).RunAsync();
  }
}