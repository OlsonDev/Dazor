using System;
using System.Collections.Generic;
using Dazor.Config;
using Dazor.Extensions;

namespace Dazor.Cli.Interaction {
  internal static class Prompt {
    private static readonly ConsoleColor PromptColor = ConsoleColor.Cyan;
    private static readonly ConsoleColor SuggestionColor = ConsoleColor.DarkGray;
    private static readonly ConsoleColor InputColor = ConsoleColor.White;

    internal static IEnumerable<string> With(string prompt) {
      yield return WithImpl(prompt);
    }

    internal static T With<T>(string prompt)
      => WithSuggestion<T>(prompt, "");

    internal static T WithSuggestion<T>(string prompt, string suggestion) {
      prompt = AdjustPromptByType<T>(prompt);
      while (true) {
        try {
          return Convert.To<T>(WithImpl(prompt, suggestion));
        } catch (Exception ex) {
          Console.Error.WriteErrorLine(ex.Message);
        }
      }
    }

    private static string AdjustPromptByType<T>(string prompt) {
      var type = typeof(T);
      if (type == typeof(bool)) return prompt + " (Y/N)";
      if (type == typeof(AutoFromClauseMode)) return prompt + " (off/on)";
      if (type == typeof(AutoJoinClauseMode)) return prompt + " (off/fk/foreign-key/convention)";
      return prompt;
    }

    private static string WithImpl(string prompt, string? suggestion = null) {
      var previousColor = Console.ForegroundColor;
      Console.ForegroundColor = PromptColor;
      Console.Write(prompt.Trim() + " ");
      if (!string.IsNullOrWhiteSpace(suggestion)) {
        Console.ForegroundColor = SuggestionColor;
        Console.Write(suggestion);
      }

      Console.ForegroundColor = InputColor;
      var value = Console.ReadLine() ?? ""; // If Ctrl+Z is pressed.
      Console.ForegroundColor = previousColor;
      return value;
    }
  }
}