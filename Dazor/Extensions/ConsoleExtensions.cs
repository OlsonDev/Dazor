using System;
using System.IO;

namespace Dazor.Extensions {
  public static class ConsoleExtensions {
    public static TextWriter WriteErrorLine(this TextWriter writer, string message) {
      var previousColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.DarkRed;
      writer.Write("Error: ");
      Console.ForegroundColor = ConsoleColor.Yellow;
      writer.WriteLine(message);
      Console.ForegroundColor = previousColor;
      return writer;
    }
  }
}