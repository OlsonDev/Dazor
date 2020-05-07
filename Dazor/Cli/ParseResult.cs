using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dazor.Cli {
  internal class ParseResult {
    internal IList<ParseError> Errors { get; set; }

    internal object Command { get; set; }

    public ParseResult() { }

    public ParseResult(IEnumerable<string> errors)
      => Errors = errors
        .Select(error => new ParseError(error))
        .ToList();

    public ParseResult(params string[] errors) : this((IEnumerable<string>)errors) { }

    internal Result Run() {
      if (!(Errors is null) && Errors.Any()) {
        foreach (var error in Errors) {
          Console.Error.WriteLine(error.Message);
        }
        return Result.ParseError;
      }
      var type = Command.GetType();
      var execute = type.GetMethod("Execute", BindingFlags.NonPublic | BindingFlags.Instance);
      return (Result)execute.Invoke(Command, null);
    }
  }
}