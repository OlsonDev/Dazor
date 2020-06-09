using System;
using System.IO;
using System.Threading.Tasks;
using Dazor.Cli.Options;
using Dazor.Config;

namespace Dazor.Cli.Commands {
  internal class UndoCommand : ICommand {
    private readonly UndoOptions _options;
    private readonly BoundConfig _config;

    public UndoCommand(UndoOptions options, BoundConfig config) {
      _options = options;
      _config = config;
    }

    public Task<Result> ExecuteAsync() {
      // TODO: Consolidate something related to this...
      // var files = Directory.GetFiles(_config.RootDirectory, "*.sql", SearchOption.AllDirectories);
      // Console.WriteLine("Files:");
      // Console.WriteLine(files);
      return Task.FromResult(Result.Success);
    }
  }
}