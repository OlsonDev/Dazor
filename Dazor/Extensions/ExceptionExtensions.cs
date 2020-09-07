using System;
using System.IO;
using System.Text;

namespace Dazor.Extensions {
  public static class ExceptionExtensions {
    public static string PrettyPrint(this Exception exception)
      => BuildLogMessage(exception, new StringBuilder(), "").TrimEnd().ToString();

    public static void PrettyPrintTo(this Exception exception, TextWriter writer)
      => PrettyPrintExceptionTo(exception, writer, "");

    private static void PrettyPrintExceptionTo(Exception ex, TextWriter writer, string indent, string? prefix = null) {
      // Does not handle cycles.
      var foregroundColor = Console.ForegroundColor;
      writer.Write(indent);
      WritePrefix(writer, prefix);
      Console.ForegroundColor = ConsoleColor.DarkRed;
      writer.Write(ex.GetType().FullName);
      writer.Write(": ");
      Console.ForegroundColor = ConsoleColor.Red;
      writer.WriteLine(ex.Message);
      writer.Write(indent);
      WritePrefix(writer, prefix);
      Console.ForegroundColor = ConsoleColor.DarkBlue;
      writer.Write("StackTrace:");
      if (ex.StackTrace is null) {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        writer.WriteLine(" (none)");
      } else {
        Console.ForegroundColor = ConsoleColor.Gray;
        writer.WriteLine();
        var lines = ex.StackTrace.SplitLines();
        var adjustedIndent = AdjustIndent(indent, prefix);
        foreach (var line in lines) {
          writer.Write(adjustedIndent);
          // TODO: Parse line into components and colorize components individually.
          // Example line:
          // at Dazor.Cli.Commands.UpgradeCommand.ExecuteAsync(Int32 executionId, SqlConnection connection) in C:\dev\Dazor\Dazor\Cli\Commands\UpgradeCommand.cs:line 40
          writer.WriteLine(line);
        }
      }
      if (ex is AggregateException aggEx) {
        writer.Write(indent);
        WritePrefix(writer, prefix);
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        writer.Write("InnerExceptions (");
        Console.ForegroundColor = ConsoleColor.Magenta;
        writer.Write(aggEx.InnerExceptions.Count);
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        writer.WriteLine("):");
        indent = IncreaseIndent(indent, prefix);
        for (var i = 0; i < aggEx.InnerExceptions.Count; i++) {
          var innerEx = aggEx.InnerExceptions[i];
          var innerExPrefix = $"{i + 1}. ";
          PrettyPrintExceptionTo(innerEx, writer, indent, innerExPrefix);
          writer.WriteLine();
        }
      } else if (ex.InnerException is not null) {
        writer.Write(indent);
        WritePrefix(writer, prefix);
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        writer.WriteLine("InnerException:");
        PrettyPrintExceptionTo(ex.InnerException, writer, IncreaseIndent(indent, prefix));
      }

      Console.ForegroundColor = foregroundColor;
    }

    private static void WritePrefix(TextWriter writer, string? prefix) {
      if (prefix is null) return;
      Console.ForegroundColor = ConsoleColor.Magenta;
      writer.Write(prefix);
    }

    private static string AdjustIndent(string indent, string? prefix)
      => indent + new string(' ', prefix?.Length ?? 0);

    private static string IncreaseIndent(string indent, string? prefix)
      => "  " + AdjustIndent(indent, prefix);

    private static StringBuilder BuildLogMessage(Exception ex, StringBuilder sb, string indent, string? prefix = null) {
      // Does not handle cycles.
      sb.Append(indent);
      if (prefix is not null) sb.Append(prefix);
      sb.Append(ex.GetType().FullName);
      sb.Append(": ");
      sb.AppendLine(ex.Message);
      sb.Append(indent);
      if (prefix is not null) sb.Append(prefix);
      sb.Append("StackTrace:");
      if (ex.StackTrace is null) {
        sb.AppendLine(" (none)");
      } else {
        sb.AppendLine();
        var adjustedIndent = AdjustIndent(indent, prefix);
        sb.AppendLinesWithIndent(ex.StackTrace, adjustedIndent);
      }
      if (ex is AggregateException aggEx) {
        sb.Append(indent);
        if (prefix is not null) sb.Append(prefix);
        sb.Append("InnerExceptions (");
        sb.Append(aggEx.InnerExceptions.Count);
        sb.AppendLine("):");
        indent = IncreaseIndent(indent, prefix);
        for (var i = 0; i < aggEx.InnerExceptions.Count; i++) {
          var innerEx = aggEx.InnerExceptions[i];
          var innerExPrefix = $"{i + 1}. ";
          BuildLogMessage(innerEx, sb, indent, innerExPrefix);
          sb.AppendLine();
        }
      } else if (ex.InnerException is not null) {
        sb.Append(indent);
        if (prefix is not null) sb.Append(prefix);
        sb.AppendLine("InnerException:");
        BuildLogMessage(ex.InnerException, sb, IncreaseIndent(indent, prefix));
      }
      return sb;
    }
  }
}