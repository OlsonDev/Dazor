using System;
using System.Text;

namespace Dazor.Extensions {
  public static class ExceptionExtensions {
    public static string PrettyPrint(this Exception exception)
      => BuildLogMessage(exception, new StringBuilder(), "").TrimEnd().ToString();

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
        sb.AppendLinesWithIndent(ex.StackTrace, indent);
      }
      if (ex is AggregateException aggEx) {
        sb.Append(indent);
        if (prefix is not null) sb.Append(prefix);
        sb.Append("InnerExceptions (");
        sb.Append(aggEx.InnerExceptions.Count);
        sb.AppendLine("):");
        indent += "  ";
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
        BuildLogMessage(ex.InnerException, sb, indent + "  ");
      }
      return sb;
    }
  }
}