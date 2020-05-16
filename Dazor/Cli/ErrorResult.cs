using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dazor.Cli {
  internal class ErrorResult : IParseResult {
    internal IList<ParseError> Errors { get; }

    internal ErrorResult(IEnumerable<string> errors)
      => Errors = errors
        .Select(error => new ParseError(error))
        .ToList();

    public ErrorResult(params string[] errors)
      : this((IEnumerable<string>)errors) { }

    public Task<Result> RunAsync() {
      var previousColor = Console.ForegroundColor;
      foreach (var error in Errors) {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Error.Write("Error: ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Error.WriteLine(error.Message);
      }
      Console.ForegroundColor = previousColor;
      return Task.FromResult(Result.ParseError);
    }
  }
}