using System;
using Dazor.Cli.Options;

namespace Dazor.Cli.Commands {
  internal class InitCommand {
    private readonly InitOptions _options;

    internal InitCommand(InitOptions options)
      => _options = options;

    internal Result Execute() {
      Console.WriteLine($"Init command with options: ConnectionString = {_options.ConnectionString}");
      return Result.Success;
    }
  }
}