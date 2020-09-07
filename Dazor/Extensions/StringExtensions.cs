using System;

namespace Dazor.Extensions {
  public static class StringExtensions {

    private static readonly string[] NewlineSeparators = new[] { "\r\n", "\r", "\n" };

    public static string[] SplitLines(this string value)
      => value.Split(NewlineSeparators, StringSplitOptions.None);
  }
}