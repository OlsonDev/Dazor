using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dazor.Extensions;

namespace Dazor.Cli {
  internal class ParseResult {
    internal IList<ParseError>? Errors { get; set; }

    internal object? Command { get; set; }

    internal ParseResult(object command)
      => Command = command;

    internal ParseResult(IEnumerable<string> errors)
      => Errors = errors
        .Select(error => new ParseError(error))
        .ToList();

    public ParseResult(params string[] errors) : this((IEnumerable<string>)errors) { }

    internal Task<Result> RunAsync() {
      if (!(Errors is null) && Errors.Any()) {
        foreach (var error in Errors) {
          Console.Error.WriteLine(error.Message);
        }
        return Task.FromResult(Result.ParseError);
      }
      // Based on our constructors, either Errors or Command must be non-null.
      var type = Command!.GetType();
      var executeAsync = type.GetMethod("ExecuteAsync", BindingFlags.NonPublic | BindingFlags.Instance);
      if (executeAsync is null) {
        Console.Error.WriteLine($"Command {Command.GetType().GetFriendlyName()} does not have an ExecuteAsync method.");
        return Task.FromResult(Result.InternalError);
      }
      var result = executeAsync.Invoke(Command, null);
      if (result is null) {
        Console.Error.WriteLine($"Command {Command.GetType().GetFriendlyName()}'s ExecuteAsync method returned null when a value was expected.");
        return Task.FromResult(Result.InternalError);
      }
      return (Task<Result>)result;
    }
  }
}