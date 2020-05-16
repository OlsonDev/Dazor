using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dazor.Extensions;

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
      foreach (var error in Errors) {
        Console.Error.WriteErrorLine(error.Message);
      }
      return Task.FromResult(Result.ParseError);
    }
  }
}