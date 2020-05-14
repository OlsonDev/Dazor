using System;
using System.Threading.Tasks;

namespace Dazor.Cli.Commands {
  internal class HelpCommand {
    internal Task<Result> ExecuteAsync() {
      Console.WriteLine($"Help command executed");
      return Task.FromResult(Result.Success);
    }
  }
}