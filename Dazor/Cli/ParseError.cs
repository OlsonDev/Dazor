namespace Dazor.Cli {
  internal class ParseError {
    internal string Message { get; set; }
    internal ParseError(string message)
      => Message = message;
  }
}