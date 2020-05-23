using System;
using System.Collections.Generic;
using System.Linq;
using Dazor.Config;
using Dazor.Extensions;

namespace Dazor.Cli {
  internal static class Convert {
    internal static T To<T>(string value) {
      var type = typeof(T);
      if (type == typeof(string)) return (T)(object)value;
      if (type == typeof(bool)) return (T)(object)ConvertToBool(value);
      if (type == typeof(AutoFromClauseMode)) return (T)(object)ConvertToAutoFromClauseMode(value);
      if (type == typeof(AutoJoinClauseMode)) return (T)(object)ConvertToAutoJoinMode(value);
      if (type == typeof(GitHookMode)) return (T)(object)ConvertToGitHookMode(value);
      throw new NotImplementedException($"{nameof(Convert)}.{nameof(To)}<{type.GetFriendlyName()}>(value) not implemented.");
    }

    internal static T To<T>(IEnumerable<string> values) {
      var type = typeof(T);
      if (type == typeof(string)) return (T)(object)string.Join(" ", values);
      if (type == typeof(bool)) return (T)(object)ConvertToBool(values.Single());
      if (type == typeof(AutoFromClauseMode)) return (T)(object)ConvertToAutoFromClauseMode(values.Single());
      if (type == typeof(AutoJoinClauseMode)) return (T)(object)ConvertToAutoJoinMode(values.Single());
      if (type == typeof(GitHookMode)) return (T)(object)ConvertToGitHookMode(values.Single());
      throw new NotImplementedException($"{nameof(Convert)}.{nameof(To)}<{type.GetFriendlyName()}>(values) not implemented.");
    }

    internal static bool ConvertToBool(string value)
      => value.ToLowerInvariant() switch {
        "y" => true,
        "yes" => true,
        "1" => true,
        "true" => true,
        "n" => false,
        "no" => false,
        "0" => false,
        "false" => false,
        _ => throw new ParseException($"Please enter y/yes/1/true/n/no/0/false."),
      };

    internal static AutoFromClauseMode ConvertToAutoFromClauseMode(string value)
      => value.ToLowerInvariant() switch {
        "on" => AutoFromClauseMode.On,
        "off" => AutoFromClauseMode.Off,
        _ => throw new ParseException($"Please enter on/off."),
      };

    internal static AutoJoinClauseMode ConvertToAutoJoinMode(string value)
      => value.ToLowerInvariant() switch {
        "off" => AutoJoinClauseMode.Off,
        "fk" => AutoJoinClauseMode.ForeignKey,
        "foreign-key" => AutoJoinClauseMode.ForeignKey,
        "convention" => AutoJoinClauseMode.Convention,
        _ => throw new ParseException($"Please enter on/fk/foreign-key/convention."),
      };

    internal static GitHookMode ConvertToGitHookMode(string value)
      => value.ToLowerInvariant() switch {
        "on" => GitHookMode.On,
        "off" => GitHookMode.Off,
        _ => throw new ParseException($"Please enter on or off."),
      };
  }
}