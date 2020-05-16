using System;

namespace Dazor.Cli {
  [Serializable]
  internal class ParseException : Exception {
    public ParseException(string? message) : base(message) { }
  }
}