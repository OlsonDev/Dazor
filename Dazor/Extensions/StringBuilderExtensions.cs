using System;
using System.Text;

namespace Dazor.Extensions {
  public static class StringBuilderExtensions {
    private static readonly string[] NewlineSeparators = new[] { "\r\n", "\r", "\n" };

    public static StringBuilder TrimEnd(this StringBuilder sb) {
      var length = sb.Length;
      if (length == 0) return sb;
      var i = length - 1;
      while (i >= 0 && char.IsWhiteSpace(sb[i])) i--;
      if (i < length - 1) sb.Length = i + 1;
      return sb;
    }

    public static StringBuilder AppendLinesWithIndent(this StringBuilder sb, string value, string indent) {
      var lines = value.Split(NewlineSeparators, StringSplitOptions.None);
      foreach (var line in lines) {
        sb.Append(indent);
        sb.AppendLine(line);
      }
      return sb;
    }
  }
}