using System.Reflection;
using System.Threading.Tasks;
using Dazor.Extensions;

namespace Dazor.Cli {
  internal class CommandResult : IParseResult {
    internal ICommand Command { get; }

    internal CommandResult(ICommand command)
      => Command = command;

    public Task<Result> RunAsync()
      => Command.ExecuteAsync();
  }
}