using System;
using System.Collections.Generic;
using Dazor.Extensions;

namespace Dazor.Cli.Interaction {
  internal static class Prompt {
    internal static IEnumerable<string> With(string prompt) {
      var previousColor = Console.ForegroundColor;
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.Write(prompt.Trim() + " ");
      Console.ForegroundColor = ConsoleColor.White;
      var value = Console.ReadLine() ?? ""; // If Ctrl+Z is pressed.
      Console.ForegroundColor = previousColor;
      yield return value;
    }

    internal static T With<T>(string prompt) {
      prompt = AdjustPromptByType<T>(prompt);
      while (true) {
        try {
          return Convert.To<T>(With(prompt));
        } catch (Exception ex) {
          Console.Error.WriteErrorLine(ex.Message);
        }
      }
    }

    private static string AdjustPromptByType<T>(string prompt) {
      var type = typeof(T);
      if (type == typeof(bool)) return prompt + " (Y/N)";
      return prompt;
    }
  }
}